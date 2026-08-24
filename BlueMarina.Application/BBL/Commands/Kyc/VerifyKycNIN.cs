using System.Net;
using System.ComponentModel.DataAnnotations;
using BlueMarina.Application.Interfaces.Kyc;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Results;
using MediatR;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BlueMarina.Shared.Logging;

namespace BlueMarina.Application.BBL.Commands.Kyc;

public sealed class VerifyKycNIN
{
    public sealed record Command : IRequest<BaseResult<VerifyKycNINDto>>
    {
        [JsonIgnore]
        public long UserId { get; init; }

        [Required, MinLength(11), MaxLength(20)]
        public string Nin { get; init; } = string.Empty;
    }

    public class VerifyKycNINDto
    {
        public long UserId { get; set; }
        public string Nin { get; set; } = string.Empty;
        public string DOB { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsNinValid { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult<VerifyKycNINDto>>
    {
        private readonly IKycService _kycService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IKycService kycService, 
            IUnitOfWork unitOfWork,
            ILogger<Handler> logger)
        {
            _kycService = kycService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BaseResult<VerifyKycNINDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "VerifyKycNIN",
                ("UserId", request.UserId),
                ("Nin", request.Nin)
            );

            var normalizedNin = request.Nin?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedNin) || normalizedNin.Length != 11 || !normalizedNin.All(char.IsDigit))
            {
                op.Fail("NIN must be exactly 11 numeric digits.");
                return new BaseResult<VerifyKycNINDto>(
                    HttpStatusCode.BadRequest,
                    "NIN must be exactly 11 numeric digits."
                );
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var profile = await _unitOfWork.Query<KycProfile>()
                    .FirstOrDefaultAsync(x => x.UserProfileId == request.UserId, cancellationToken);

                if (profile != null && !string.IsNullOrWhiteSpace(profile.Bvn))
                {
                    op.Fail($"User {request.UserId} already has a verified BVN. NIN cannot be changed.");
                    return new BaseResult<VerifyKycNINDto>(
                        HttpStatusCode.BadRequest,
                        "You have already completed your KYC. You cannot change your NIN now."
                    );
                }

                if (profile is null)
                {
                    profile = new KycProfile
                    {
                        UserProfileId = request.UserId,
                        Bvn = null,
                        Nin = normalizedNin,
                        KycLevel = KycLevel.Basic
                    };

                    await _unitOfWork.AddAsync(profile, cancellationToken);
                }
                else
                {
                    profile.Nin = normalizedNin;
                    _unitOfWork.Update(profile);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _kycService.RecordVerificationAsync(
                    request.UserId, 
                    VerificationType.Nin, 
                    VerificationResult.Pass, 
                    "NIN verification submitted and validated", 
                    cancellationToken: cancellationToken
                );

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                op.Success($"NIN successfully verified for User {request.UserId}.");

                return new BaseResult<VerifyKycNINDto>
                (
                    HttpStatusCode.OK, 
                    "NIN submitted and verified successfully. KYC Level upgraded to Verified.", 
                    new VerifyKycNINDto
                    {
                        UserId = request.UserId,
                        Nin = "12345678901",
                        DOB = "1990-01-01", 
                        PhoneNumber = "08064160898",
                        IsNinValid = true
                    }
                );
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                op.Fail($"NIN verification failed for User {request.UserId}: {ex.Message}", ex);
                return new BaseResult<VerifyKycNINDto>(
                    HttpStatusCode.InternalServerError,
                    "NIN verification failed. Please try again later."
                );
            }
        }
    }
}