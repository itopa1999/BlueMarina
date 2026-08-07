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
        public long UserId { get; set; }
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

            var userExists = await _unitOfWork.Query<UserProfile>()
                .AsNoTracking()
                .Where(up => up.Email == request.Email || up.PhoneNumber == request.PhoneNumber)
                .Select(up => new { up.Id, up.Email, up.FirstName, up.PhoneNumber })
                .FirstOrDefaultAsync(cancellationToken);

            if (userExists == null)
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
                    UserProfileId = userExists.Id,
                    OtpCode = otpCode,
                    Purpose = OtpPurpose.AccountVerification,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(2),
                    IsUsed = false
                };

                await _unitOfWork.AddAsync(otp, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);


                await _emailService.SendOtpAsync(userExists?.FirstName ?? "Customer", userExists.Email, otpCode, cancellationToken);
                await _smsService.SendOtpAsync(userExists.PhoneNumber, otpCode, cancellationToken);

                op.Success($"OTP resent successfully for user {userExists.Id}");

                return new BaseResult<ResendVerificationOtpResponseDto>(
                    HttpStatusCode.OK,
                    "Verification OTP resent successfully.",
                    new ResendVerificationOtpResponseDto
                    {
                        UserId = userExists.Id,
                        Data = "Data from payload here"
                    });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                op.Fail($"Resend OTP failed for user {userExists.Id}", ex);
                throw;
            }
        }
    }
}
