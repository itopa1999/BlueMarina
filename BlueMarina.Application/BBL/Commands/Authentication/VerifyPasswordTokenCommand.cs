using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Common.Dtos;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Logging;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Authentication;

public sealed class VerifyPasswordTokenCommand
{
    public class Command : IRequest<BaseResult<VerifyPasswordTokenResponseDto>>
    {
        public Guid UserId { get; init; }

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
        private readonly ILogger<Handler> _logger;
        

        public Handler(IUnitOfWork unitOfWork, 
        IIdentityService identityService, 
        ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<BaseResult<VerifyPasswordTokenResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "VerifyPasswordToken",
                ("UserId", request.UserId.ToString())
            );

            IdentityDto? user = null;

            var (userExists, _, userDetails) = await _identityService.GetUserDetailsByUserIDAsync(request.UserId);
            if (!userExists)
            {
                op.Fail($"User not found: {request.UserId}");
                return new BaseResult<VerifyPasswordTokenResponseDto>(
                    HttpStatusCode.BadRequest,
                    "User not Found");
            }

            user = userDetails;

            var otpVerification = await _unitOfWork.Query<OtpVerification>()
                .Where(x => x.UserId == request.UserId
                            && x.Purpose == OtpPurpose.PasswordReset.ToString()
                            && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (otpVerification is null)
            {
                op.Fail($"No valid OTP found for user {request.UserId}");
                return new BaseResult<VerifyPasswordTokenResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid OTP");
            }

            if (otpVerification.OtpCode != request.Token)
            {
                op.Fail($"Invalid OTP code for user {request.UserId}");
                return new BaseResult<VerifyPasswordTokenResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid OTP code.");
            }

            if (otpVerification.ExpiresAt < DateTime.UtcNow)
            {
                op.Fail($"OTP expired for user {request.UserId}");
                return new BaseResult<VerifyPasswordTokenResponseDto>(
                    HttpStatusCode.BadRequest,
                    "OTP has expired. Please request a new one.");
            }

            // This check is redundant because we already filtered !x.IsUsed, but keep for safety.
            if (otpVerification.IsUsed)
            {
                op.Fail($"OTP already used for user {request.UserId}");
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

                op.Success($"Password reset OTP verified for user {request.UserId}");
                return new BaseResult<VerifyPasswordTokenResponseDto>(
                    HttpStatusCode.OK,
                    "OTP verified successfully. You can now reset your password.",
                    new VerifyPasswordTokenResponseDto
                    {
                        UserId = request.UserId,
                        IsVerified = true
                    });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                op.Fail($"OTP verification failed for user {request.UserId}", ex);
                throw;
            }
        }
    }
}