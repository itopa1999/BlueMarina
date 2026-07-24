using MediatR;
using BlueMarina.Shared.Results;
using BlueMarina.Application.Interfaces.Identity;
using System.Net;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Constants;
using BlueMarina.Application.Interfaces.Persistence;
using System.ComponentModel.DataAnnotations;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Application.Interfaces.Notification;


namespace BlueMarina.Application.BBL.Commands.Authentication;

public sealed class RegisterCommand
{
    public class Command: IRequest<BaseResult<RegistrationResponseDto>>
    {
        [MinLength(2), MaxLength(50)]
        public string FirstName { get; init; } = string.Empty;

        [MinLength(2), MaxLength(50)]
        public string LastName { get; init; } = string.Empty;
        [EmailAddress, MaxLength(100)]
        public string Email { get; init; } = string.Empty;

        public string PhoneNumber { get; init; } = string.Empty;

        [MinLength(8)]
        public string Password { get; init; } = string.Empty;
    }

    public class RegistrationResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string Phone {get; set;}
        public string FullName { get; set; }
        public string Data { get; set; }

    }

    public class Handler : IRequestHandler<Command, BaseResult<RegistrationResponseDto>>
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
        public async Task<BaseResult<RegistrationResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            
            if (await _identityService.EmailExistsAsync(request.Email))
            {
                return new BaseResult<RegistrationResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Email already exists.");
            }

            if (await _identityService.PhoneNumberExistsAsync(
                request.PhoneNumber))
            {
                return new BaseResult<RegistrationResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Phone number already exists.");
            }
            
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try{
                var (success, errorMessage, userId) = await _identityService.CreateUserAsync(
                    request.Email,
                    request.PhoneNumber,
                    request.Password);
                
                if (!success)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return new BaseResult<RegistrationResponseDto>
                    (
                        HttpStatusCode.BadRequest,
                        errorMessage
                    );
                }

                var (roleSuccess, roleError) = await _identityService.AddToRoleAsync(userId, Roles.Customer);
                if (!roleSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return new BaseResult<RegistrationResponseDto>(HttpStatusCode.BadRequest, roleError );
                }

                var otpCode = _otpService.GenerateOtp();

                var otp = new OtpVerification
                {
                    UserId = userId,
                    OtpCode = otpCode,
                    Purpose = OtpPurpose.AccountVerification,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(2)
                };

                var userProfile = new UserProfile
                {
                    UserId = userId,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                };

                await _unitOfWork.AddAsync(userProfile);
                await _unitOfWork.AddAsync(otp);


                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                await _emailService.SendOtpAsync(
                    request.Email,
                    otpCode);

                await _smsService.SendOtpAsync(
                    request.PhoneNumber,
                    otpCode);

                return new BaseResult<RegistrationResponseDto>(
                    HttpStatusCode.Created,
                    "Account created successfully.",
                    new RegistrationResponseDto
                    {
                        UserId = userId,
                        Email = request.Email,
                        Phone = request.PhoneNumber,
                        FullName = userProfile.FullName,
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
    