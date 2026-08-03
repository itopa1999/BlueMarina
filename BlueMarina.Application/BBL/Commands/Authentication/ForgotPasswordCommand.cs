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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Authentication;

public sealed class ForgotPasswordCommand
{
    public class Command : IRequest<BaseResult<ForgotPasswordResponseDto>>
    {
        [EmailAddress, MaxLength(100)]
        public string? Email { get; init; }

        public string? PhoneNumber { get; init; }
    }

    public class ForgotPasswordResponseDto
    {
        public Guid UserId { get; set; }
        public string Data { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult<ForgotPasswordResponseDto>>
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

        public async Task<BaseResult<ForgotPasswordResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var identifier = !string.IsNullOrWhiteSpace(request.Email) ? request.Email : request.PhoneNumber;
            using var op = OperationLogger.Start(_logger, "ForgotPassword",
                ("Identifier", identifier ?? "unknown")
            );

            if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                op.Fail("Neither email nor phone number provided.");
                return new BaseResult<ForgotPasswordResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Either Email or Phone Number must be provided.");
            }

            Guid? userId = null;

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var (success, _, id) = await _identityService.GetUserIdByEmailAsync(request.Email);
                if (success) userId = id;
            }
            else if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                var (success, _, id) = await _identityService.GetUserIdByPhoneAsync(request.PhoneNumber);
                if (success) userId = id;
            }

            if (!userId.HasValue)
            {
                op.Fail("User not found (identifier provided but not in system).");
                return new BaseResult<ForgotPasswordResponseDto>(
                    HttpStatusCode.OK,
                    "If an account exists with this email or phone number, a reset OTP will be sent.",
                    new ForgotPasswordResponseDto
                    {
                        UserId = Guid.Empty,
                        Data = "If an account exists, OTP has been sent."
                    });
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var otpCode = _otpService.GenerateOtp();
                var otp = new OtpVerification
                {
                    UserId = userId.Value,
                    OtpCode = otpCode,
                    Purpose = OtpPurpose.PasswordReset,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(2),
                    CreatedAt = DateTime.UtcNow,
                    IsUsed = false
                };

                await _unitOfWork.AddAsync(otp);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                // if (!string.IsNullOrWhiteSpace(request.Email))
                // {
                //     await _emailService.SendForgotPasswordOtpAsync(request.Email, otpCode);
                // }
                // if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                // {
                //     await _smsService.SendOtpAsync(request.PhoneNumber, otpCode);
                // }

                op.Success($"OTP sent to user {userId} via {(request.Email != null ? "email" : "SMS")}");

                return new BaseResult<ForgotPasswordResponseDto>(
                    HttpStatusCode.OK,
                    "If an account exists with this email or phone number, a reset OTP will be sent.",
                    new ForgotPasswordResponseDto
                    {
                        UserId = userId.Value,
                        Data = "OTP sent successfully."
                    });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                op.Fail($"Forgot password failed for user {userId}", ex);
                throw;
            }
        }
    }
}