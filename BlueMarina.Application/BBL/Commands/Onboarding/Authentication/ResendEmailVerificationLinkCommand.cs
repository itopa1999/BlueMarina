using MediatR;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Application.Interfaces.Notification;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.ExternalAPI;
using static BlueMarina.Application.Interfaces.Security.IOtpService;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Authentication;

public sealed class ResendEmailVerificationLinkCommand
{
    public class Command : IRequest<BaseResult<ResendEmailVerificationLinkResponseDto>>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;
    }

    public class ResendEmailVerificationLinkResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult<ResendEmailVerificationLinkResponseDto>>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOtpService _otpService;
        private readonly INotificationQueue _notificationQueue;
        private readonly ExternalApiSettings _settings;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            IOtpService otpService,
            INotificationQueue notificationQueue,
            IOptions<ExternalApiSettings> settings,
            ILogger<Handler> logger)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _otpService = otpService;
            _notificationQueue = notificationQueue;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<BaseResult<ResendEmailVerificationLinkResponseDto>> Handle(
            Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "ResendEmailVerificationLink",
                ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                op.Warning("Missing user id.");
                return new BaseResult<ResendEmailVerificationLinkResponseDto>(
                    HttpStatusCode.BadRequest, "User id is required.");
            }

            var user = await _identityService.GetByIdentifierAsync(
                request.UserId, cancellationToken);

            if (user is null)
            {
                op.Warning("User not found.");
                return new BaseResult<ResendEmailVerificationLinkResponseDto>(
                    HttpStatusCode.NotFound, "User not found.");
            }

            if (user.EmailConfirmed)
            {
                op.Warning("Email already verified.");
                return new BaseResult<ResendEmailVerificationLinkResponseDto>(
                    HttpStatusCode.Conflict, "Email is already verified.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                op.Warning("No email on file.");
                return new BaseResult<ResendEmailVerificationLinkResponseDto>(
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
                return new BaseResult<ResendEmailVerificationLinkResponseDto>(
                    HttpStatusCode.TooManyRequests,
                    "Too many failed attempts. Please try again later.");
            }

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            var emailOtp = _otpService.GenerateOtp(OtpPurpose.SignUpVerification.ToString());

            await _unitOfWork.AddAsync(new OtpCode
            {
                UserId = user.PublicId,
                CodeHash = emailOtp.HashedOtp,
                Channel = OtpChannel.Email,
                Purpose = OtpPurpose.SignUpVerification,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(emailOtp.ExpiringMinutes),
                CreatedAt = DateTimeOffset.UtcNow,
                IsUsed = false,
                Attempts = 0
            }, cancellationToken);

            var now = DateTimeOffset.UtcNow;
            await _unitOfWork
                .Query<OtpCode>()
                .Where(x => x.UserId == user.PublicId
                            && x.Purpose == OtpPurpose.SignUpVerification
                            && x.Channel == OtpChannel.Email
                            && !x.IsUsed)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.IsUsed, true)
                    .SetProperty(p => p.UsedAt, now)
                    .SetProperty(p => p.ConsumedAt, now), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            var verificationLink = BuildEmailVerificationLink(user.PublicId, emailOtp.RawOtp);

            _notificationQueue.QueueEmailVerificationLink(
                firstName: string.IsNullOrWhiteSpace(user.FirstName) ? "there" : user.FirstName,
                email: user.Email!,
                link: verificationLink,
                expiry: $"{emailOtp.ExpiringMinutes} minutes");

            op.Success($"Email verification link resent for {user.PublicId}.");

            return new BaseResult<ResendEmailVerificationLinkResponseDto>(
                HttpStatusCode.OK,
                "Verification email resent successfully.",
                new ResendEmailVerificationLinkResponseDto
                {
                    UserId = user.PublicId,
                    Email = user.Email!,
                });
        }

        private string BuildEmailVerificationLink(string publicId, string rawOtp)
        {
            return $"{_settings.BackendBaseUrl.TrimEnd('/')}/api/v1/auth/verify-email" +
                   $"?userId={Uri.EscapeDataString(publicId)}" +
                   $"&code={Uri.EscapeDataString(rawOtp)}";
        }
    }
}