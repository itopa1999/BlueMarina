using MediatR;
using System.Net;
using System.ComponentModel.DataAnnotations;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static BlueMarina.Application.Interfaces.Security.IOtpService;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Authentication;

public sealed class VerifyEmailCommand
{
    public class Command : IRequest<BaseResult<VerifyEmailResponseDto>>
    {
        [Required, MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Code { get; init; } = string.Empty;
    }

    public class VerifyEmailResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult<VerifyEmailResponseDto>>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOtpService _otpService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            IOtpService otpService,
            ILogger<Handler> logger)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _otpService = otpService;
            _logger = logger;
        }

        public async Task<BaseResult<VerifyEmailResponseDto>> Handle(
            Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "VerifyEmail",
                ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.Code))
            {
                op.Warning("Missing user id or code.");
                return new BaseResult<VerifyEmailResponseDto>(
                    HttpStatusCode.BadRequest,
                    "User id and verification code are required.");
            }

            var user = await _identityService.GetByIdentifierAsync(
                request.UserId, cancellationToken);

            if (user is null)
            {
                op.Warning("User not found.");
                return new BaseResult<VerifyEmailResponseDto>(
                    HttpStatusCode.NotFound, "User not found.");
            }

            if (user.EmailConfirmed)
            {
                op.Warning("Email already verified.");
                return new BaseResult<VerifyEmailResponseDto>(
                    HttpStatusCode.Conflict, "Email is already verified.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                op.Warning("No email on file.");
                return new BaseResult<VerifyEmailResponseDto>(
                    HttpStatusCode.BadRequest, "No email address on file.");
            }

            var lockout = await _otpService.CheckAsync(
                user.PublicId,
                OtpPurpose.SignUpVerification,
                OtpChannel.Email,
                cancellationToken);

            if (lockout == OtpAttemptResult.LockedOut)
            {
                op.Warning("Email verification lockout active.");
                return new BaseResult<VerifyEmailResponseDto>(
                    HttpStatusCode.TooManyRequests,
                    "Too many failed attempts. Please try again later.");
            }

            var otp = await _unitOfWork
                .Query<OtpCode>()
                .Where(x => x.UserId == user.PublicId
                            && x.Purpose == OtpPurpose.SignUpVerification
                            && x.Channel == OtpChannel.Email
                            && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (otp is null)
            {
                op.Warning("No active email OTP found.");
                return new BaseResult<VerifyEmailResponseDto>(
                    HttpStatusCode.BadRequest,
                    "No active verification link found. Please request a new one.");
            }

            if (otp.ExpiresAt < DateTimeOffset.UtcNow)
            {
                op.Warning("Email OTP expired.");
                return new BaseResult<VerifyEmailResponseDto>(
                    HttpStatusCode.BadRequest,
                    "This verification link has expired. Please request a new one.");
            }

            var submittedHash = _otpService.Hash(
                request.Code,
                OtpPurpose.SignUpVerification.ToString());

            if (!string.Equals(otp.CodeHash, submittedHash, StringComparison.OrdinalIgnoreCase))
            {
                var failResult = await _otpService.RegisterFailureAsync(
                    user.PublicId,
                    OtpPurpose.SignUpVerification,
                    OtpChannel.Email,
                    cancellationToken);

                if (failResult == OtpAttemptResult.LockedNow)
                {
                    op.Warning("Email OTP locked after max failed attempts.");
                    return new BaseResult<VerifyEmailResponseDto>(
                        HttpStatusCode.TooManyRequests,
                        "Too many failed attempts. Your email verification has been temporarily locked.");
                }

                op.Warning("Invalid verification code.");
                return new BaseResult<VerifyEmailResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid verification link.");
            }

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            var (markSuccess, markError) = await _identityService
                .MarkEmailAsVerifiedAsync(user.Id);

            if (!markSuccess)
            {
                op.Warning($"Failed to mark email verified: {markError}");
                return new BaseResult<VerifyEmailResponseDto>(
                    HttpStatusCode.BadRequest, markError);
            }

            otp.IsUsed = true;
            otp.UsedAt = DateTimeOffset.UtcNow;
            otp.ConsumedAt = DateTimeOffset.UtcNow;

            var now = DateTimeOffset.UtcNow;
            await _unitOfWork
                .Query<OtpCode>()
                .Where(x => x.UserId == user.PublicId
                            && x.Purpose == OtpPurpose.SignUpVerification
                            && x.Channel == OtpChannel.Email
                            && !x.IsUsed
                            && x.Id != otp.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.IsUsed, true)
                    .SetProperty(p => p.UsedAt, now)
                    .SetProperty(p => p.ConsumedAt, now), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _otpService.ResetAsync(
                user.PublicId,
                OtpPurpose.SignUpVerification,
                OtpChannel.Email,
                cancellationToken);

            await tx.CommitAsync(cancellationToken);

            op.Success($"Email verified for {user.PublicId}.");

            return new BaseResult<VerifyEmailResponseDto>(
                HttpStatusCode.OK,
                "Email verified successfully.",
                new VerifyEmailResponseDto
                {
                    UserId = user.PublicId,
                    EmailConfirmed = true,
                });
        }
    }
}