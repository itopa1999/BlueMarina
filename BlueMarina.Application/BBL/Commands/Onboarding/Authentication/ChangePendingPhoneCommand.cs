using MediatR;
using System.Net;
using System.ComponentModel.DataAnnotations;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Application.Interfaces.Notification;
using BlueMarina.Application.Helpers;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Authentication;

public sealed class ChangePendingPhoneCommand
{
    public class Command : IRequest<BaseResult<ChangePendingPhoneResponseDto>>
    {
        [Required, MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string NewPhoneNumber { get; init; } = string.Empty;
    }

    public class ChangePendingPhoneResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string NextStep { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult<ChangePendingPhoneResponseDto>>
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

        public async Task<BaseResult<ChangePendingPhoneResponseDto>> Handle(
            Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "ChangePendingPhone",
                ("UserId", request.UserId),
                ("NewPhone", request.NewPhoneNumber));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                op.Warning("Missing user id.");
                return new BaseResult<ChangePendingPhoneResponseDto>(
                    HttpStatusCode.BadRequest, "User id is required.");
            }

            var normalizedPhone = ExtensionHelpers.Normalize(request.NewPhoneNumber);
            if (normalizedPhone is null)
            {
                op.Warning("Invalid phone number format.");
                return new BaseResult<ChangePendingPhoneResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Please provide a valid Nigerian phone number.");
            }

            var user = await _identityService.GetByIdentifierAsync(
                request.UserId, cancellationToken);

            if (user is null)
            {
                op.Warning("User not found.");
                return new BaseResult<ChangePendingPhoneResponseDto>(
                    HttpStatusCode.NotFound, "User not found.");
            }

            if (user.PhoneNumberConfirmed)
            {
                op.Warning("User phone already confirmed.");
                return new BaseResult<ChangePendingPhoneResponseDto>(
                    HttpStatusCode.Conflict,
                    "Your phone number is already verified and cannot be changed here.");
            }

            var phoneExists = await _identityService.PhoneExistsAsync(
                normalizedPhone,
                excludeUserId: user.Id,
                cancellationToken: cancellationToken);

            if (phoneExists)
            {
                op.Warning($"Phone already in use: {normalizedPhone}");
                return new BaseResult<ChangePendingPhoneResponseDto>(
                    HttpStatusCode.Conflict, "Phone number is already in use.");
            }

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            var (updateSuccess, updateError) = await _identityService
                .UpdatePendingPhoneAsync(request.UserId, normalizedPhone, cancellationToken);

            if (!updateSuccess)
            {
                op.Warning($"Phone update failed: {updateError}");
                return new BaseResult<ChangePendingPhoneResponseDto>(
                    HttpStatusCode.BadRequest, updateError);
            }

            var smsOtp = _otpService.GenerateOtp(OtpPurpose.SignUpVerification.ToString());

            await _unitOfWork.AddAsync(new OtpCode
            {
                UserId = request.UserId,
                CodeHash = smsOtp.HashedOtp,
                Channel = OtpChannel.Sms,
                Purpose = OtpPurpose.SignUpVerification,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                CreatedAt = DateTimeOffset.UtcNow,
                IsUsed = false,
                Attempts = 0
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            _notificationQueue.QueueOtpDelivery(
                string.Empty,
                string.Empty,
                normalizedPhone,
                smsOtp.RawOtp);

            op.Success($"Pending phone changed to {normalizedPhone} for user {request.UserId}.");

            return new BaseResult<ChangePendingPhoneResponseDto>(
                HttpStatusCode.OK,
                "Phone number updated. A new verification code has been sent.",
                new ChangePendingPhoneResponseDto
                {
                    UserId = request.UserId,
                    Phone = normalizedPhone,
                    NextStep = NextSteps.VerifyContact
                });
        }
    }
}