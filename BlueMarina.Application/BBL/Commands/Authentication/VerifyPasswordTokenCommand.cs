
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

public sealed class VerifyPasswordTokenCommand
{
    public class Command : IRequest<BaseResult<VerifyPasswordTokenResponseDto>>
    {
        public Guid UserId {get; init;}

        [MinLength(6), MaxLength(6)]
        public string Token { get; init; } = string.Empty;
    }

    public class VerifyPasswordTokenResponseDto
    {
        public Guid UserId { get; set; }
        public bool IsVerified { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult<VerifyPasswordTokenResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;

        public Handler(IUnitOfWork unitOfWork, IIdentityService identityService)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
        }
        public async Task<BaseResult<VerifyPasswordTokenResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            bool success = await _identityService.IsUserExisting(request.UserId);
            if (!success)
            {
                return new BaseResult<VerifyPasswordTokenResponseDto>
                (
                    HttpStatusCode.BadRequest,
                    "User not Found"
                );
            }

            
            var otpVerification = await _unitOfWork.Query<OtpVerification>()
            .Where(x =>  x.UserId == request.UserId && x.Purpose == OtpPurpose.PasswordReset.ToString() && !x.IsUsed)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

            if (otpVerification is null)
            {
                return new BaseResult<VerifyPasswordTokenResponseDto>
                (
                    HttpStatusCode.BadRequest,
                    "Invalid Otp"
                );
            }

            if (otpVerification.OtpCode != request.Token)
            {
                return new BaseResult<VerifyPasswordTokenResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid OTP code.");
            }

            if (otpVerification.ExpiresAt < DateTime.UtcNow)
            {
                return new BaseResult<VerifyPasswordTokenResponseDto>(
                    HttpStatusCode.BadRequest,
                    "OTP has expired. Please request a new one.");
            }

            if (otpVerification.IsUsed)
            {
                return new BaseResult<VerifyPasswordTokenResponseDto>(
                    HttpStatusCode.BadRequest,
                    "This OTP has already been used.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {

                otpVerification.IsUsed = true;
                otpVerification.UsedAt = DateTime.UtcNow;
                _unitOfWork.Update(otpVerification);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return new BaseResult<VerifyPasswordTokenResponseDto>(
                    HttpStatusCode.OK,
                    "OTP verified successfully. You can now reset your password.",
                    new VerifyPasswordTokenResponseDto
                    {
                        UserId = request.UserId,
                        IsVerified = true
                    });
            
            } catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    
    }
}

