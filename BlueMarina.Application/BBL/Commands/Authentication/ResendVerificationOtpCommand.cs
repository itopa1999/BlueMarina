using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Notification;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Logging;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Authentication;

public sealed class ResendVerificationOtpCommand
{
    public class Command : IRequest<BaseResult<ResendVerificationOtpResponseDto>>
    {
        [EmailAddress]
        public string? Email { get; init; }

        public string? PhoneNumber { get; init; }
    }

    public class ResendVerificationOtpResponseDto
    {
        public Guid UserId { get; set; }
        public string Data { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult<ResendVerificationOtpResponseDto>>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            IOtpService otpService,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<Handler> logger)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _otpService = otpService;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task<BaseResult<ResendVerificationOtpResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var identifier = !string.IsNullOrWhiteSpace(request.Email) ? request.Email : request.PhoneNumber;
            using var op = OperationLogger.Start(_logger, "ResendVerificationOtp",
                ("Identifier", identifier ?? "unknown")
            );

            if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                op.Fail("Neither email nor phone number provided.");
                return new BaseResult<ResendVerificationOtpResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Either Email or Phone Number must be provided.");
            }

            Guid? userId = null;

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var (success, errorMessage, id) = await _identityService.GetUserIdByEmailAsync(request.Email);
                if (!success)
                {
                    op.Fail($"User not found by email: {request.Email}");
                    return new BaseResult<ResendVerificationOtpResponseDto>(
                        HttpStatusCode.BadRequest,
                        errorMessage);
                }
                userId = id;
            }
            else if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                var (success, errorMessage, id) = await _identityService.GetUserIdByPhoneAsync(request.PhoneNumber);
                if (!success)
                {
                    op.Fail($"User not found by phone: {request.PhoneNumber}");
                    return new BaseResult<ResendVerificationOtpResponseDto>(
                        HttpStatusCode.BadRequest,
                        errorMessage);
                }
                userId = id;
            }

            if (!userId.HasValue)
            {
                op.Fail("User not found.");
                return new BaseResult<ResendVerificationOtpResponseDto>(
                    HttpStatusCode.BadRequest,
                    "User not found.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var otpCode = _otpService.GenerateOtp();

                var otp = new OtpVerification
                {
                    UserId = userId.Value,
                    OtpCode = otpCode,
                    Purpose = OtpPurpose.AccountVerification,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(2),
                    IsUsed = false
                };

                await _unitOfWork.AddAsync(otp);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    await _emailService.SendOtpAsync(request.Email, otpCode);
                }

                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    await _smsService.SendOtpAsync(request.PhoneNumber, otpCode);
                }

                op.Success($"OTP resent successfully for user {userId}");

                return new BaseResult<ResendVerificationOtpResponseDto>(
                    HttpStatusCode.OK,
                    "Verification OTP resent successfully.",
                    new ResendVerificationOtpResponseDto
                    {
                        UserId = userId.Value,
                        Data = "Data from payload here"
                    });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                op.Fail($"Resend OTP failed for user {userId}", ex);
                throw;
            }
        }
    }
}