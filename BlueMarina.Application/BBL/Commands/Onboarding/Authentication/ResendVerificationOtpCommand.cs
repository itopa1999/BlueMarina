using MediatR;
using System.Net;
using System.ComponentModel.DataAnnotations;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Application.Interfaces.Notification;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Authentication;

public sealed class ResendVerificationOtpCommand
{
    public class Command : IRequest<BaseResult<ResendVerificationOtpResponseDto>>
    {
        [Required, MaxLength(100)]
        public string UserId { get; set; } = string.Empty;
    }

    public class ResendVerificationOtpResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string NextStep { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult<ResendVerificationOtpResponseDto>>
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

        public async Task<BaseResult<ResendVerificationOtpResponseDto>> Handle(
            Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "ResendVerificationOtp",
                ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                op.Warning("Missing user id.");
                return new BaseResult<ResendVerificationOtpResponseDto>(
                    HttpStatusCode.BadRequest, "User id is required.");
            }

            var user = await _identityService.GetByIdentifierAsync(
                request.UserId, cancellationToken);

            if (user is null)
            {
                op.Warning("User not found.");
                return new BaseResult<ResendVerificationOtpResponseDto>(
                    HttpStatusCode.NotFound, "User not found.");
            }

            if (user.PhoneNumberConfirmed)
            {
                op.Warning("Phone already verified.");
                return new BaseResult<ResendVerificationOtpResponseDto>(
                    HttpStatusCode.Conflict, "Phone number is already verified.");
            }

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                op.Warning("No phone number on file.");
                return new BaseResult<ResendVerificationOtpResponseDto>(
                    HttpStatusCode.BadRequest, "No phone number on file.");
            }

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            var otp = _otpService.GenerateOtp(OtpPurpose.SignUpVerification.ToString());

            await _unitOfWork.AddAsync(new OtpCode
            {
                UserId = user.PublicId,
                CodeHash = otp.HashedOtp,
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
                user.PhoneNumber,
                otp.RawOtp);

            op.Success($"OTP resent via Sms for user {user.PublicId}.");

            return new BaseResult<ResendVerificationOtpResponseDto>(
                HttpStatusCode.OK,
                "Verification code resent via SMS.",
                new ResendVerificationOtpResponseDto
                {
                    UserId = user.PublicId,
                    Channel = OtpChannel.Sms.ToString(),
                    NextStep = NextSteps.VerifyContact
                });
        }
    }
}