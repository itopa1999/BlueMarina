using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier3;

public sealed class SubmitTaxDetailsCommand
{
    public class Command : IRequest<BaseResult>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Occupation { get; set; } = string.Empty;

        public string? Employer { get; set; }

        [Required]
        public bool IsPep { get; set; }

        public List<TaxResidencyInputDto> TaxResidencies { get; set; } = new();
    }

    public class TaxResidencyInputDto
    {
        [Required, StringLength(2, MinimumLength = 2)]
        public string CountryCode { get; set; } = string.Empty;

        public string? Tin { get; set; }

        public TaxNoTinReason? NoTinReason { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IIdentityService identityService, 
            IUnitOfWork unitOfWork, 
            ILogger<Handler> logger)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "SubmitFinalDetails", ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
                return new BaseResult(HttpStatusCode.Unauthorized, "User context is missing.");

            foreach (var tr in request.TaxResidencies)
            {
                if (string.IsNullOrWhiteSpace(tr.Tin) && !tr.NoTinReason.HasValue)
                {
                    return new BaseResult(HttpStatusCode.BadRequest, 
                        $"Please provide a TIN or select a reason for not having one for {tr.CountryCode}.");
                }
            }

            var (success, error) = await _identityService.UpdateFinalDetailsAsync(
                request.UserId, request.Occupation, request.Employer, request.IsPep, cancellationToken);

            if (!success)
                return new BaseResult(HttpStatusCode.BadRequest, error);

            var existingResidencies = await _unitOfWork.Query<TaxResidency>()
                .Where(x => x.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingResidencies)
            {
                _unitOfWork.Remove(existing);
            }

            foreach (var tr in request.TaxResidencies)
            {
                var taxResidency = new TaxResidency
                {
                    UserId = request.UserId,
                    CountryCode = tr.CountryCode.ToUpperInvariant(),
                    Tin = tr.Tin,
                    NoTinReason = tr.NoTinReason,
                    DeclaredAt = DateTimeOffset.UtcNow
                };
                await _unitOfWork.AddAsync(taxResidency, cancellationToken);
            }

            var progress = await _unitOfWork.Query<UserOnboardingProgress>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (progress == null)
                return new BaseResult(HttpStatusCode.NotFound, "Onboarding progress not found.");

            progress.Tier3_OccupationDone = true;
            progress.Tier3_TaxResidencyDone = request.TaxResidencies.Any();

            if (progress.Tier3_PersonalDetailsDone && 
                progress.Tier3_AddressDone && 
                progress.Tier3_NextOfKinDone && 
                progress.Tier3_BankAccountDone && 
                progress.Tier3_OccupationDone && 
                progress.Tier3_TaxResidencyDone)
            {
                progress.Tier3_IsCompleted = true;
                
                await _identityService.UpdateUserTierAsync(request.UserId, AccountTier.Tier3, cancellationToken);
            }

            _unitOfWork.Update(progress);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            op.Success("Tax details and tax residency saved.");

            return new BaseResult(HttpStatusCode.OK, "Tax details saved successfully.");
        }
    }
}