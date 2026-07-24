
using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Application.BBL.Commands.Authentication;

public sealed class VerifyAccountCommand
{
    public class Command : IRequest<BaseResult<VerifyAccountResponseDto>>
    {
        [EmailAddress]
        public string? Email { get; init; }

        public string? PhoneNumber {get; init;}

        [MinLength(6), MaxLength(6)]
        public string OtpCode { get; init; } = string.Empty;
    }

    public class VerifyAccountResponseDto
    {
        public Guid UserId { get; set; }

        public bool IsAccountVerified { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult<VerifyAccountResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;

        public Handler(IUnitOfWork unitOfWork, IIdentityService identityService)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
        }
        public async Task<BaseResult<VerifyAccountResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            try{
                if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    return new BaseResult<VerifyAccountResponseDto>(
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
                        return new BaseResult<VerifyAccountResponseDto>
                        (
                            HttpStatusCode.BadRequest,
                            errorMessage
                        );
                    }

                    userId = id;
                } else if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    var (success, errorMessage, id) = await _identityService.GetUserIdByPhoneAsync(request.PhoneNumber);
                    if (!success)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return new BaseResult<VerifyAccountResponseDto>
                        (
                            HttpStatusCode.BadRequest,
                            errorMessage
                        );
                    }

                    userId = id;
                }

                if (!userId.HasValue)
                {
                    return new BaseResult<VerifyAccountResponseDto>(
                        HttpStatusCode.BadRequest,
                        "User not found.");
                }


                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var otpVerification = await _unitOfWork.Query<OtpVerification>()
                .Where(x =>  x.UserId == userId && x.Purpose == OtpPurpose.AccountVerification.ToString())
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

                if (otpVerification is null)
                {
                    return new BaseResult<VerifyAccountResponseDto>
                    (
                        HttpStatusCode.BadRequest,
                        "Invalid Otp"
                    );
                }

                if (otpVerification.OtpCode != request.OtpCode)
                {
                    return new BaseResult<VerifyAccountResponseDto>(
                        HttpStatusCode.BadRequest,
                        "Invalid OTP code.");
                }

                if (otpVerification.ExpiresAt < DateTime.UtcNow)
                {
                    return new BaseResult<VerifyAccountResponseDto>(
                        HttpStatusCode.BadRequest,
                        "OTP has expired. Please request a new one.");
                }

                if (otpVerification.IsUsed)
                {
                    return new BaseResult<VerifyAccountResponseDto>(
                        HttpStatusCode.BadRequest,
                        "This OTP has already been used.");
                }

                var (markSuccess, markError) = await _identityService.MarkEmailAndPhoneAsVerifiedAsync(userId);
                if (!markSuccess)
                {
                    return new BaseResult<VerifyAccountResponseDto>(
                        HttpStatusCode.BadRequest,
                        markError);
                }

                otpVerification.IsUsed = true;
                otpVerification.UsedAt = DateTime.UtcNow;
                _unitOfWork.Update(otpVerification);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return new BaseResult<VerifyAccountResponseDto>(
                    HttpStatusCode.OK,
                    "Email verified successfully.",
                    new VerifyAccountResponseDto
                    {
                        UserId = userId.Value,
                        IsAccountVerified = true
                    });
            } catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}