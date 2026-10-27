using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Helpers;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Notification;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Authentication;

public sealed class ForgotPasswordCommand
{
    public class Command : IRequest<BaseResult<ForgotPasswordResponseDto>>
    {
        [Required]
        public string Identifier { get; init; } = string.Empty;
    }

    public class ForgotPasswordResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string NextStep { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult<ForgotPasswordResponseDto>>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOtpService _otpService;
        private readonly INotificationQueue _notificationQueue;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            IOtpService otpService,
            INotificationQueue notificationQueue,
            ILogger<Handler> logger)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _otpService = otpService;
            _notificationQueue = notificationQueue;
            _logger = logger;
        }

        public async Task<BaseResult<ForgotPasswordResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "ForgotPassword",
                ("Identifier", request.Identifier));

            if (string.IsNullOrWhiteSpace(request.Identifier))
            {
                return new BaseResult<ForgotPasswordResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Email or phone number is required.");
            }

            var isEmail = request.Identifier.Contains('@');
            var identifierToUse = request.Identifier;

            if (!isEmail)
            {
                var normalizedPhone = ExtensionHelpers.Normalize(request.Identifier);
                if (normalizedPhone is null)
                {
                    op.Warning("Invalid phone number format.");
                    return new BaseResult<ForgotPasswordResponseDto>(
                        HttpStatusCode.BadRequest,
                        "Please provide a valid Nigerian phone number.");
                }
                identifierToUse = normalizedPhone;
            }

            var user = await _identityService.GetByIdentifierAsync(identifierToUse, cancellationToken);

            if (user is null)
            {
                op.Warning("User not found for identifier.");
                return new BaseResult<ForgotPasswordResponseDto>(
                    HttpStatusCode.OK,
                    "If an account exists, a reset OTP will be sent.",
                    new ForgotPasswordResponseDto { NextStep = "VerifyPasswordResetOtp" });
            }

            if (await _identityService.IsLockedOutAsync(user.Id, cancellationToken))
            {
                var lockoutEnd = await _identityService.GetLockoutEndAsync(user.Id, cancellationToken);
                var minutes = lockoutEnd.HasValue
                    ? Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes)
                    : 15;

                op.Warning($"User {user.PublicId} is locked out.");
                return new BaseResult<ForgotPasswordResponseDto>(
                    HttpStatusCode.TooManyRequests,
                    $"Account is temporarily locked. Please try again in {minutes} minute(s).");
            }

            if (user.Status == UserStatus.Suspended || user.Status == UserStatus.Closed)
            {
                op.Warning($"User {user.PublicId} attempted password reset but status is {user.Status}.");
                return new BaseResult<ForgotPasswordResponseDto>(
                    HttpStatusCode.Forbidden,
                    "This account has been suspended or closed. Please contact support.");
            }

            if (isEmail && !user.EmailConfirmed)
            {
                op.Warning($"Email not verified for user {user.PublicId}.");
                return new BaseResult<ForgotPasswordResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Please verify your email before resetting your password.");
            }

            if (!isEmail && !user.PhoneNumberConfirmed)
            {
                op.Warning($"Phone not verified for user {user.PublicId}.");
                return new BaseResult<ForgotPasswordResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Please verify your phone number before resetting your password.");
            }

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            var otp = _otpService.GenerateOtp(OtpPurpose.PasswordReset.ToString());

            await _unitOfWork.AddAsync(new OtpCode
            {
                UserId = user.PublicId,
                CodeHash = otp.HashedOtp,
                Channel = OtpChannel.Sms,
                Purpose = OtpPurpose.PasswordReset,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(otp.ExpiringMinutes),
                CreatedAt = DateTimeOffset.UtcNow,
                IsUsed = false,
                Attempts = 0
            }, cancellationToken);

            await _unitOfWork.AddAsync(new OtpCode
            {
                UserId = user.PublicId,
                CodeHash = otp.HashedOtp,
                Channel = OtpChannel.Email,
                Purpose = OtpPurpose.PasswordReset,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(otp.ExpiringMinutes),
                CreatedAt = DateTimeOffset.UtcNow,
                IsUsed = false,
                Attempts = 0
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                _notificationQueue.QueueOtpDelivery(
                    string.Empty,
                    string.Empty,
                    user.PhoneNumber,
                    otp.RawOtp);
            }

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                _notificationQueue.QueueOtpDelivery(
                    string.Empty,
                    string.Empty,
                    user.Email,
                    otp.RawOtp);
            }

            op.Success($"Password reset OTP sent to user {user.PublicId}.");

            return new BaseResult<ForgotPasswordResponseDto>(
                HttpStatusCode.OK,
                "If an account exists, a reset OTP will be sent.",
                new ForgotPasswordResponseDto {
                    UserId = user.PublicId,
                    NextStep = SignUpNextSteps.VerifyPasswordResetOtp
                    });
        }
    }
}