using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Notification;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Results;
using MediatR;

namespace BlueMarina.Application.BBL.Commands.Authentication;

public sealed class ForgotPasswordCommand
{
    public class Command: IRequest<BaseResult<ForgotPasswordResponseDto>>
    {
        [EmailAddress, MaxLength(100)]
        public string? Email { get; init; }

        public string? PhoneNumber { get; init; }
    }

    public class ForgotPasswordResponseDto
    {
        public Guid UserId { get; set; }
        public string Data { get; set; }

    }
    public class Handler : IRequestHandler<Command, BaseResult<ForgotPasswordResponseDto>>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;

        public Handler(
            IIdentityService identityService, IUnitOfWork unitOfWork,
            IOtpService otpService, IEmailService emailService, ISmsService smsService )
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _otpService = otpService;
            _emailService = emailService;
            _smsService = smsService;
        }
        public async Task<BaseResult<ForgotPasswordResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return new BaseResult<ForgotPasswordResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Either Email or Phone Number must be provided.");
            }

            Guid? userId = null;

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var (success, errorMessage, id) = await _identityService.GetUserIdByEmailAsync(request.Email);
                if (!success)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return new BaseResult<ForgotPasswordResponseDto>
                    (
                        HttpStatusCode.BadRequest,
                        "If an account exists with this email or phone number, a reset OTP will be sent"
                    );
                }

                userId = id;
            } else if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                var (success, errorMessage, id) = await _identityService.GetUserIdByPhoneAsync(request.PhoneNumber);
                if (!success)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return new BaseResult<ForgotPasswordResponseDto>
                    (
                        HttpStatusCode.BadRequest,
                        "If an account exists with this email or phone number, a reset OTP will be sent"
                    );
                }

                userId = id;
            }

            if (!userId.HasValue)
            {
                return new BaseResult<ForgotPasswordResponseDto>(
                    HttpStatusCode.BadRequest,
                    "If an account exists with this email or phone number, a reset OTP will be sent.");
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
                    ExpiresAt = DateTime.UtcNow.AddMinutes(2)
                };
                await _unitOfWork.AddAsync(otp);
                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    await _emailService.SendOtpAsync(request.Email, otpCode);
                }

                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    await _smsService.SendOtpAsync(request.PhoneNumber, otpCode);
                }

                return new BaseResult<ForgotPasswordResponseDto>(
                    HttpStatusCode.OK,
                    "If an account exists with this email or phone number, a reset OTP will be sent",  
                    new ForgotPasswordResponseDto
                    {
                        UserId = userId.Value,
                        Data = "Data from payload here"
                    });

            }catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}