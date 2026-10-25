using MediatR;
using BlueMarina.Application.Interfaces.Identity;
using System.Net;
using BlueMarina.Application.Interfaces.Persistence;
using System.ComponentModel.DataAnnotations;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Application.Interfaces.Notification;
using BlueMarina.Shared.Logging;
using Microsoft.Extensions.Logging;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Application.Helpers;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Constants;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Authentication;

public sealed class RegisterCommand
{
    public class Command : IRequest<BaseResult<RegistrationResponseDto>>
    {
        [EmailAddress, MaxLength(100)]
        public string Email { get; init; } = string.Empty;

        [Required, MaxLength(20)]
        public string PhoneNumber { get; init; } = string.Empty;

        [MinLength(8)]
        public string Password { get; init; } = string.Empty;

        [MaxLength(50)]
        public string? ReferralCode { get; init; }
    }

    public class RegistrationResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string NextStep { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult<RegistrationResponseDto>>
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

        public async Task<BaseResult<RegistrationResponseDto>> Handle(
            Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "RegisterUser",
                ("Email", request.Email),
                ("Phone", request.PhoneNumber));

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var normalizedPhoneNumber = ExtensionHelpers.Normalize(request.PhoneNumber);

            if (normalizedPhoneNumber is null)
            {
                op.Warning("Invalid phone number format.");
                return new BaseResult<RegistrationResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Please provide a valid Nigerian phone number.");
            }

            var emailExists = await _identityService.EmailExistsAsync(
                normalizedEmail, cancellationToken: cancellationToken);
            if (emailExists)
            {
                op.Warning($"Email already exists: {normalizedEmail}");
                return new BaseResult<RegistrationResponseDto>(
                    HttpStatusCode.Conflict, "Email is already in use.");
            }

            var phoneExists = await _identityService.PhoneExistsAsync(
                normalizedPhoneNumber, cancellationToken: cancellationToken);
            if (phoneExists)
            {
                op.Warning($"Phone already exists: {normalizedPhoneNumber}");
                return new BaseResult<RegistrationResponseDto>(
                    HttpStatusCode.Conflict, "Phone number is already in use.");
            }

            long? referredByUserId = null;
            string? referredByCode = null;

            if (!string.IsNullOrWhiteSpace(request.ReferralCode))
            {
                var (found, referrerId) = await _identityService.FindUserByReferralCodeAsync(
                    request.ReferralCode, cancellationToken);

                if (!found)
                {
                    op.Warning($"Invalid referral code: {request.ReferralCode}");
                    return new BaseResult<RegistrationResponseDto>(
                        HttpStatusCode.BadRequest,
                        "Invalid referral code.");
                }

                referredByUserId = referrerId;
                referredByCode = request.ReferralCode.Trim().ToUpperInvariant();
            }

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            var (success, errorMessage, publicId, userId) =
                await _identityService.CreateUserAsync(
                    normalizedEmail,
                    normalizedPhoneNumber,
                    request.Password,
                    referredByUserId,
                    referredByCode);

            if (!success)
            {
                op.Warning($"User creation failed: {errorMessage}");
                return new BaseResult<RegistrationResponseDto>(
                    HttpStatusCode.BadRequest, errorMessage);
            }

            if (referredByUserId.HasValue && referredByUserId.Value == userId)
            {
                op.Warning($"Self-referral blocked for user {publicId}.");
                return new BaseResult<RegistrationResponseDto>(
                    HttpStatusCode.BadRequest,
                    "You cannot refer yourself.");
            }

            var (roleSuccess, roleError) = await _identityService.AddToRoleAsync(
                userId, Roles.Customer);
            if (!roleSuccess)
            {
                op.Warning($"Role assignment failed: {roleError}");
                return new BaseResult<RegistrationResponseDto>(
                    HttpStatusCode.BadRequest, roleError);
            }

            var credential = new Credential
            {
                UserId = publicId,
                LoginPinHash = null,
                TransactionPinHash = null,
                BiometricEnabled = false
            };
            await _unitOfWork.AddAsync(credential, cancellationToken);

            var smsOtp = _otpService.GenerateOtp(OtpPurpose.SignUpVerification.ToString());
            await _unitOfWork.AddAsync(new OtpCode
            {
                UserId = publicId,
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
                normalizedPhoneNumber,
                smsOtp.RawOtp);
                

            op.Success($"User {publicId} registered successfully.");

            return new BaseResult<RegistrationResponseDto>(
                HttpStatusCode.Created,
                "Account created successfully.",
                new RegistrationResponseDto
                {
                    UserId = publicId,
                    Email = normalizedEmail,
                    Phone = normalizedPhoneNumber,
                    NextStep = NextSteps.VerifyContact
                });
        }
    }
}