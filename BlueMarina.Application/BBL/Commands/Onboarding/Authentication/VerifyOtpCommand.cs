using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static BlueMarina.Application.Interfaces.Security.IOtpService;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Authentication;

public sealed class VerifyOtpCommand
{
    public class Command : IRequest<BaseResult<VerifyOtpResponseDto>>
    {
        [Required]
        public string UserId { get; init; } = string.Empty;

        [Required, MinLength(6), MaxLength(6)]
        public string Token { get; init; } = string.Empty;
    }

    public class VerifyOtpResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public string ChallengeKey { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult<VerifyOtpResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly IOtpService _otpService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IUnitOfWork unitOfWork, 
            IIdentityService identityService, 
            IOtpService otpService,
            ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _otpService = otpService;
            _logger = logger;
        }

        public async Task<BaseResult<VerifyOtpResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "VerifyOtp",
                ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return new BaseResult<VerifyOtpResponseDto>(
                    HttpStatusCode.BadRequest,
                    "UserId is required.");
            }

            var user = await _identityService.GetByIdentifierAsync(request.UserId, cancellationToken);

            if (user is null)
            {
                op.Warning("User not found.");
                return new BaseResult<VerifyOtpResponseDto>(
                    HttpStatusCode.NotFound, "User not found.");
            }

            var otp = await _unitOfWork.Query<OtpCode>()
                .Where(x => x.UserId == user.PublicId && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (otp is null)
            {
                op.Warning("No active OTP found.");
                return new BaseResult<VerifyOtpResponseDto>(
                    HttpStatusCode.BadRequest, 
                    "No active OTP found. Please request a new one.");
            }

            if (otp.ExpiresAt < DateTimeOffset.UtcNow)
            {
                op.Warning("OTP expired.");
                return new BaseResult<VerifyOtpResponseDto>(
                    HttpStatusCode.BadRequest, 
                    "OTP has expired. Please request a new one.");
            }

            var lockout = await _otpService.CheckAsync(
                user.PublicId, 
                otp.Purpose, 
                otp.Channel,
                cancellationToken);

            if (lockout == OtpAttemptResult.LockedOut)
            {
                op.Warning("OTP lockout active.");
                return new BaseResult<VerifyOtpResponseDto>(
                    HttpStatusCode.TooManyRequests, 
                    "Too many failed attempts. Please try again later.");
            }

            var submittedHash = _otpService.Hash(request.Token, otp.Purpose.ToString());

            if (!string.Equals(otp.CodeHash, submittedHash, StringComparison.OrdinalIgnoreCase))
            {
                var failResult = await _otpService.RegisterFailureAsync(
                    user.PublicId, 
                    otp.Purpose, 
                    otp.Channel, 
                    cancellationToken);

                if (failResult == OtpAttemptResult.LockedNow)
                {
                    op.Warning("OTP locked after max failed attempts.");
                    return new BaseResult<VerifyOtpResponseDto>(
                        HttpStatusCode.TooManyRequests, 
                        "Too many failed attempts. Please try again later.");
                }

                op.Warning("Invalid OTP code.");
                return new BaseResult<VerifyOtpResponseDto>(
                    HttpStatusCode.BadRequest, 
                    "Invalid OTP code.");
            }

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            otp.IsUsed = true;
            otp.UsedAt = DateTimeOffset.UtcNow;
            otp.ConsumedAt = DateTimeOffset.UtcNow;
            
            var now = DateTimeOffset.UtcNow;
            await _unitOfWork.Query<OtpCode>()
                .Where(x => x.UserId == user.PublicId
                            && x.Purpose == otp.Purpose
                            && !x.IsUsed
                            && x.Id != otp.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.IsUsed, true)
                    .SetProperty(p => p.UsedAt, now)
                    .SetProperty(p => p.ConsumedAt, now), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            await _otpService.ResetAsync(
                user.PublicId, 
                otp.Purpose, 
                otp.Channel, 
                cancellationToken);

            await tx.CommitAsync(cancellationToken);

            op.Success($"OTP verified successfully for {otp.Purpose}.");

            return new BaseResult<VerifyOtpResponseDto>(
                HttpStatusCode.OK,
                "OTP verified successfully.",
                new VerifyOtpResponseDto
                {
                    UserId = user.PublicId,
                    IsVerified = true,
                    ChallengeKey = otp.Id.ToString()
                });
        }
    }
}