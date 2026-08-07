using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Common.Dtos;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Notification;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Logging;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Authentication;

public sealed class VerifyAccountCommand
{
    public class Command : IRequest<BaseResult<VerifyAccountResponseDto>>
    {
        [EmailAddress]
        public string? Email { get; init; }

        public string? PhoneNumber { get; init; }

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
        private readonly ILogger<Handler> _logger;
        private readonly IEmailService _emailService;

        public Handler(IUnitOfWork unitOfWork, 
        IIdentityService identityService, 
        ILogger<Handler> logger,
        IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<BaseResult<VerifyAccountResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var identifier = !string.IsNullOrWhiteSpace(request.Email) ? request.Email : request.PhoneNumber;
            using var op = OperationLogger.Start(_logger, "VerifyAccount",
                ("Identifier", identifier ?? "unknown")
            );

            if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                op.Fail("Neither email nor phone number provided.");
                return new BaseResult<VerifyAccountResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Either Email or Phone Number must be provided.");
            }

            Guid? userId = null;
            IdentityDto? user = null;

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var (success, _, userDetails) = await _identityService.GetUserDetailsByEmailAsync(request.Email);
                if (success) userId = userDetails.UserId;
                user = userDetails;
            }
            else if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                var (success, _, userDetails) = await _identityService.GetUserDetailsByPhoneAsync(request.PhoneNumber);
                if (success) userId = userDetails.UserId;
                user = userDetails;
            }

            if (!userId.HasValue)
            {
                op.Fail("User not found.");
                return new BaseResult<VerifyAccountResponseDto>(
                    HttpStatusCode.BadRequest,
                    "User not found.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var otpVerification = await _unitOfWork.Query<OtpVerification>()
                    .Where(x => x.UserId == userId
                                && x.Purpose == OtpPurpose.AccountVerification.ToString()
                                && !x.IsUsed)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (otpVerification is null)
                {
                    op.Fail($"No valid OTP found for user {userId}");
                    return new BaseResult<VerifyAccountResponseDto>(
                        HttpStatusCode.BadRequest,
                        "Invalid OTP. Please request a new one.");
                }

                if (otpVerification.OtpCode != request.OtpCode)
                {
                    op.Fail($"Invalid OTP code provided for user {userId}");
                    return new BaseResult<VerifyAccountResponseDto>(
                        HttpStatusCode.BadRequest,
                        "Invalid OTP code.");
                }

                if (otpVerification.ExpiresAt < DateTime.UtcNow)
                {
                    op.Fail($"OTP expired for user {userId}");
                    return new BaseResult<VerifyAccountResponseDto>(
                        HttpStatusCode.BadRequest,
                        "OTP has expired. Please request a new one.");
                }

                // This check is redundant because we already filtered !x.IsUsed, but keep it safe.
                if (otpVerification.IsUsed)
                {
                    op.Fail($"OTP already used for user {userId}");
                    return new BaseResult<VerifyAccountResponseDto>(
                        HttpStatusCode.BadRequest,
                        "This OTP has already been used.");
                }

                var (markSuccess, markError) = await _identityService.MarkEmailAndPhoneAsVerifiedAsync(userId);
                if (!markSuccess)
                {
                    op.Fail($"Failed to mark account as verified for user {userId}: {markError}");
                    return new BaseResult<VerifyAccountResponseDto>(
                        HttpStatusCode.BadRequest,
                        markError);
                }

                otpVerification.IsUsed = true;
                otpVerification.UsedAt = DateTime.UtcNow;
                _unitOfWork.Update(otpVerification);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var userProfile = await _unitOfWork.Query<UserProfile>()
                    .AsNoTracking()
                    .Where(up => up.UserId == userId)
                    .Select(up => new { up.FirstName })
                    .FirstOrDefaultAsync(cancellationToken);

                await _emailService.SendWelcomeEmailAsync(userProfile?.FirstName ?? "Customer", user.Email);

                op.Success($"Account verified successfully for user {userId}");
                return new BaseResult<VerifyAccountResponseDto>(
                    HttpStatusCode.OK,
                    "Account verified successfully.",
                    new VerifyAccountResponseDto
                    {
                        UserId = userId.Value,
                        IsAccountVerified = true
                    });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                op.Fail($"Account verification failed for user {userId}", ex);
                throw;
            }
        }
    }
}
