using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
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

public sealed class ForgotLoginPinCommand
{
    public class Command : IRequest<BaseResult<ForgotLoginPinResponseDto>>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Password { get; init; } = string.Empty;
    }

    public class ForgotLoginPinResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string NextStep { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult<ForgotLoginPinResponseDto>>
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

        public async Task<BaseResult<ForgotLoginPinResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "ForgotLoginPin",
                ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return new BaseResult<ForgotLoginPinResponseDto>(
                    HttpStatusCode.Unauthorized,
                    "User context is missing.");
            }

            var permissions = await _identityService.GetPermissionsAsync(
                request.UserId,
                cancellationToken);

            if (permissions is null)
            {
                op.Error($"Permissions unavailable for user {request.UserId}.");
                return new BaseResult<ForgotLoginPinResponseDto>(
                    HttpStatusCode.InternalServerError,
                    "We couldn't verify your account permissions right now. Please try again in a moment.");
            }

            if (!permissions.CanPerformSensitiveOperations)
            {
                op.Warning(
                    $"PIN reset blocked for user {request.UserId}. " +
                    $"Status: {permissions.StatusLabel}, " +
                    $"Reason: {permissions.RestrictionReason}");

                var userMessage = string.IsNullOrWhiteSpace(permissions.StatusDescription)
                    ? "Your account is currently restricted from performing this action. Please contact support."
                    : permissions.StatusDescription;

                return new BaseResult<ForgotLoginPinResponseDto>(
                    HttpStatusCode.Locked,
                    userMessage);
            }

            var user = await _identityService.GetByIdentifierAsync(request.UserId, cancellationToken);

            if (user is null)
            {
                op.Warning("User not found.");
                return new BaseResult<ForgotLoginPinResponseDto>(
                    HttpStatusCode.NotFound,
                    "User not found.");
            }

            if (await _identityService.IsLockedOutAsync(user.Id, cancellationToken))
            {
                var lockoutEnd = await _identityService.GetLockoutEndAsync(user.Id, cancellationToken);
                var minutes = lockoutEnd.HasValue
                    ? Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes)
                    : 15;

                op.Warning($"User {user.PublicId} is locked out.");
                return new BaseResult<ForgotLoginPinResponseDto>(
                    HttpStatusCode.TooManyRequests,
                    $"Account is temporarily locked. Please try again in {minutes} minute(s).");
            }

            var passwordIsValid = await _identityService.CheckPasswordAsync(user.Id, request.Password);
            
            if (!passwordIsValid)
            {
                await _identityService.RecordFailedAccessAsync(user.Id, cancellationToken);
                op.Warning($"Invalid password for user {user.PublicId} during PIN reset.");
                
                if (await _identityService.IsLockedOutAsync(user.Id, cancellationToken))
                {
                    var lockoutEnd = await _identityService.GetLockoutEndAsync(user.Id, cancellationToken);
                    var minutes = lockoutEnd.HasValue 
                        ? Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes) 
                        : 15;
                        
                    return new BaseResult<ForgotLoginPinResponseDto>(
                        HttpStatusCode.TooManyRequests,
                        $"Account is temporarily locked due to too many failed attempts. Please try again in {minutes} minute(s)."
                    );
                }
                
                return new BaseResult<ForgotLoginPinResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid password."
                );
            }

            await _identityService.ResetFailedAccessAsync(user.Id, cancellationToken);

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            var otp = _otpService.GenerateOtp(OtpPurpose.PinReset.ToString());

            await _unitOfWork.AddAsync(new OtpCode
            {
                UserId = user.PublicId,
                CodeHash = otp.HashedOtp,
                Channel = OtpChannel.Sms,
                Purpose = OtpPurpose.PinReset,
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
                Purpose = OtpPurpose.PinReset,
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

            op.Success($"PIN reset OTP sent to user {user.PublicId}.");

            return new BaseResult<ForgotLoginPinResponseDto>(
                HttpStatusCode.OK,
                "OTP sent successfully. Please check your email and SMS.",
                new ForgotLoginPinResponseDto
                {
                    UserId = user.PublicId,
                    NextStep = SignUpNextSteps.VerifyPinResetOtp
                });
        }
    }
}