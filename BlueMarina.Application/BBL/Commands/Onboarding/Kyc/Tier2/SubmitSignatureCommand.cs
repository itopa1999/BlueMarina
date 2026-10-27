using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Helpers;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Storage;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier2;

public sealed class SubmitSignatureCommand
{
    public class Command : IRequest<BaseResult>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public IFormFile SignatureFile { get; set; } = null!;

        [Required]
        public bool AcceptAccountAgreement { get; set; }

        [Required]
        public bool AcceptPrivacyPolicy { get; set; }

        public bool OptInMarketUpdates { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IUnitOfWork unitOfWork, 
            IIdentityService identityService,
            IFileStorageService fileStorageService,
            ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "SubmitSignature", ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
                return new BaseResult(HttpStatusCode.Unauthorized, "User context is missing.");

            var (isValid, Message) = _fileStorageService.ValidateFile(
                request.SignatureFile.FileName, 
                request.SignatureFile.ContentType, 
                request.SignatureFile.Length);

            if (!isValid)
            {
                return new BaseResult(
                    HttpStatusCode.BadRequest, Message);
            }

            if (!request.AcceptAccountAgreement || !request.AcceptPrivacyPolicy)
                return new BaseResult(HttpStatusCode.BadRequest, "You must accept both agreements to proceed.");

            var progress = await _unitOfWork.Query<UserOnboardingProgress>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            var (canProceed, errorMessage) = progress.CanDoTier2Signature();
            
            if (!canProceed)
                return new BaseResult(HttpStatusCode.BadRequest, errorMessage);

            var activeAgreements = await (
                from av in _unitOfWork.Query<AgreementVersion>()
                join aa in _unitOfWork.Query<AgreementAcceptance>() 
                    on new { VersionId = av.Id, request.UserId } 
                    equals new { VersionId = aa.AgreementVersionId, aa.UserId } into accepted
                from aa in accepted.DefaultIfEmpty()
                where (av.Type == AgreementType.AccountAgreement || 
                       av.Type == AgreementType.PrivacyPolicy || 
                       av.Type == AgreementType.TermsOfUse) 
                      && av.IsCurrent
                select new 
                {
                    av.Type,
                    av.Version,
                    VersionId = av.Id,
                    IsAccepted = aa != null
                }
            ).ToListAsync(cancellationToken);

            var accountAgreement = activeAgreements.FirstOrDefault(x => x.Type == AgreementType.AccountAgreement);
            var privacyPolicy = activeAgreements.FirstOrDefault(x => x.Type == AgreementType.PrivacyPolicy);
            var termsOfUse = activeAgreements.FirstOrDefault(x => x.Type == AgreementType.TermsOfUse);

            if (accountAgreement == null || privacyPolicy == null || termsOfUse == null)
                return new BaseResult(HttpStatusCode.InternalServerError, "Agreements are not configured correctly.");

            if (accountAgreement.IsAccepted || privacyPolicy.IsAccepted || termsOfUse.IsAccepted)
                return new BaseResult(HttpStatusCode.Conflict, "Agreements have already been accepted.");

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            var existingSignature = await _unitOfWork.Query<Signature>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (existingSignature == null)
            {
                await _unitOfWork.AddAsync(new Signature
                {
                    UserId = request.UserId,
                    SpecimenImageUrl = "http://localhost:5014/swagger/index.html",
                    SignedAt = DateTimeOffset.UtcNow
                }, cancellationToken);
            }
            else
            {
                existingSignature.SpecimenImageUrl = "http://localhost:5014/swagger/index.html";
                existingSignature.SignedAt = DateTimeOffset.UtcNow;
                _unitOfWork.Update(existingSignature);
            }

            var signatureTimestamp = DateTimeOffset.UtcNow;

            await _unitOfWork.AddAsync(new AgreementAcceptance
            {
                UserId = request.UserId,
                AgreementVersionId = accountAgreement.VersionId,
                AcceptedAt = signatureTimestamp
            }, cancellationToken);

            await _unitOfWork.AddAsync(new AgreementAcceptance
            {
                UserId = request.UserId,
                AgreementVersionId = privacyPolicy.VersionId,
                AcceptedAt = signatureTimestamp
            }, cancellationToken);

            await _unitOfWork.AddAsync(new AgreementAcceptance
            {
                UserId = request.UserId,
                AgreementVersionId = termsOfUse.VersionId,
                AcceptedAt = signatureTimestamp
            }, cancellationToken);

            if (request.OptInMarketUpdates)
            {
                var existingConsent = await _unitOfWork.Query<Consent>()
                    .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.Type == ConsentType.MarketingUpdates, cancellationToken);

                if (existingConsent == null)
                {
                    await _unitOfWork.AddAsync(new Consent
                    {
                        UserId = request.UserId,
                        Type = ConsentType.MarketingUpdates,
                        Version = accountAgreement.Version, // Saving "v1"
                        Granted = true
                    }, cancellationToken);
                }
                else
                {
                    existingConsent.Granted = true;
                    existingConsent.Version = accountAgreement.Version;
                    _unitOfWork.Update(existingConsent);
                }
            }

            progress!.Tier2_SignatureDone = true;

            if (progress.Tier1_AccountDone && progress.Tier2_SelfieDone && progress.Tier2_IdDone && progress.Tier2_SignatureDone)
            {
                progress.Tier2_IsCompleted = true;

                var (updateSuccess, updateError) = await _identityService.UpdateUserTierAsync(
                    request.UserId, AccountTier.Tier2, cancellationToken);

                if (!updateSuccess)
                {
                    op.Warning($"Failed to update user tier: {updateError}");
                    return new BaseResult(HttpStatusCode.InternalServerError, "Failed to finalize tier upgrade.");
                }
            }

            _unitOfWork.Update(progress);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            op.Success("Signature and agreements saved successfully.");

            return new BaseResult(HttpStatusCode.OK, "Signature and agreements saved successfully.");
        }
    }
}