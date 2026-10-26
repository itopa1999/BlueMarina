using MediatR;
using System.Net;
using System.ComponentModel.DataAnnotations;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using static BlueMarina.Application.Interfaces.Security.IOtpService;
using BlueMarina.Shared.Constants;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Authentication;

public sealed class VerifyAccountCommand
{
    public class Command : IRequest<BaseResult<VerifyAccountResponseDto>>
    {
        [Required, MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(6)]
        public string OtpCode { get; init; } = string.Empty;
    }

    public class VerifyAccountResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsAccountVerified { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTimeOffset AccessTokenExpiresAt { get; set; }
        public DateTimeOffset RefreshTokenExpiresAt { get; set; }
        public string NextStep { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult<VerifyAccountResponseDto>>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOtpService _otpService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            IOtpService otpService,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenService refreshTokenService,
            ILogger<Handler> logger)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _otpService = otpService;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        public async Task<BaseResult<VerifyAccountResponseDto>> Handle(
            Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "VerifyAccount",
                ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.OtpCode))
            {
                op.Warning("Missing user id or OTP code.");
                return new BaseResult<VerifyAccountResponseDto>(
                    HttpStatusCode.BadRequest,
                    "User id and OTP code are required.");
            }

            var user = await _identityService.GetByIdentifierAsync(
                request.UserId, cancellationToken);

            if (user is null)
            {
                op.Warning("User not found.");
                return new BaseResult<VerifyAccountResponseDto>(
                    HttpStatusCode.NotFound, "User not found.");
            }

            if (user.PhoneNumberConfirmed)
            {
                op.Warning("Phone already verified.");
                return new BaseResult<VerifyAccountResponseDto>(
                    HttpStatusCode.Conflict, "Account is already verified.");
            }

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                op.Warning("No phone number on file.");
                return new BaseResult<VerifyAccountResponseDto>(
                    HttpStatusCode.BadRequest, "No phone number on file.");
            }

            var lockout = await _otpService.CheckAsync(
                user.PublicId,
                OtpPurpose.SignUpVerification,
                OtpChannel.Sms,
                cancellationToken);

            if (lockout == OtpAttemptResult.LockedOut)
            {
                op.Warning("OTP lockout active.");
                return new BaseResult<VerifyAccountResponseDto>(
                    HttpStatusCode.TooManyRequests,
                    "Too many failed attempts. Please try again later.");
            }

            // 2. Load most recent active OTP
            var otp = await _unitOfWork
                .Query<OtpCode>()
                .Where(x => x.UserId == user.PublicId
                            && x.Purpose == OtpPurpose.SignUpVerification
                            && x.Channel == OtpChannel.Sms
                            && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (otp is null)
            {
                var failResult = await _otpService.RegisterFailureAsync(
                    user.PublicId,
                    OtpPurpose.SignUpVerification,
                    OtpChannel.Sms,
                    cancellationToken);

                if (failResult == OtpAttemptResult.LockedNow)
                {
                    op.Warning("OTP locked after max failed attempts.");
                    return new BaseResult<VerifyAccountResponseDto>(
                        HttpStatusCode.TooManyRequests,
                        "Too many failed attempts. Your phone number has been temporarily locked.");
                }
                
                op.Warning("No active OTP found.");
                return new BaseResult<VerifyAccountResponseDto>(
                    HttpStatusCode.BadRequest,
                    "No OTP found. Please request a new one.");
            }

            if (otp.ExpiresAt < DateTimeOffset.UtcNow)
            {
                op.Warning("OTP expired.");
                return new BaseResult<VerifyAccountResponseDto>(
                    HttpStatusCode.BadRequest,
                    "OTP has expired. Please request a new one.");
            }

            // 3. Hash submitted code and compare
            var submittedHash = _otpService.Hash(
                request.OtpCode,
                OtpPurpose.SignUpVerification.ToString());

            var matches = string.Equals(
                otp.CodeHash, submittedHash, StringComparison.OrdinalIgnoreCase);

            if (!matches)
            {
                var failResult = await _otpService.RegisterFailureAsync(
                    user.PublicId,
                    OtpPurpose.SignUpVerification,
                    OtpChannel.Sms,
                    cancellationToken);

                if (failResult == OtpAttemptResult.LockedNow)
                {
                    op.Warning("OTP locked after max failed attempts.");
                    return new BaseResult<VerifyAccountResponseDto>(
                        HttpStatusCode.TooManyRequests,
                        "Too many failed attempts. Your phone number has been temporarily locked.");
                }

                op.Warning("Invalid OTP.");
                return new BaseResult<VerifyAccountResponseDto>(
                    HttpStatusCode.BadRequest, "Invalid OTP code.");
            }

            // 4. OTP valid — begin verification inside a transaction
            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            var (markSuccess, markError) = await _identityService.MarkPhoneAsVerifiedAsync(
                user.Id);

            if (!markSuccess)
            {
                op.Warning($"Failed to mark account verified: {markError}");
                return new BaseResult<VerifyAccountResponseDto>(
                    HttpStatusCode.BadRequest, markError);
            }

            otp.IsUsed = true;
            otp.UsedAt = DateTimeOffset.UtcNow;
            otp.ConsumedAt = DateTimeOffset.UtcNow;

            var now = DateTimeOffset.UtcNow;
            await _unitOfWork
                .Query<OtpCode>()
                .Where(x => x.UserId == user.PublicId
                            && x.Purpose == OtpPurpose.SignUpVerification
                            && !x.IsUsed
                            && x.Id != otp.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.IsUsed, true)
                    .SetProperty(p => p.UsedAt, now)
                    .SetProperty(p => p.ConsumedAt, now), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _otpService.ResetAsync(
                user.PublicId,
                OtpPurpose.SignUpVerification,
                OtpChannel.Sms,
                cancellationToken);

            var roles = await _identityService.GetRolesAsync(user.Id);

            var jwtResult = _jwtTokenGenerator.GenerateToken(
                user.Id,
                user.PublicId,
                user.FirstName,
                user.OtherNames,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.FullName,
                roles);

            var (rawRefreshToken, refreshTokenEntity) = await _refreshTokenService
                .CreateAsync(user.PublicId, cancellationToken);

            await _unitOfWork.AddAsync(refreshTokenEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            op.Success($"Account verified successfully for user {user.PublicId}.");

            return new BaseResult<VerifyAccountResponseDto>(
                HttpStatusCode.OK,
                "Account verified successfully.",
                new VerifyAccountResponseDto
                {
                    UserId = user.PublicId,
                    IsAccountVerified = true,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = rawRefreshToken,
                    AccessTokenExpiresAt = jwtResult.ExpiresAt,
                    RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt,
                    NextStep = SignUpNextSteps.CompleteProfile
                });
        }
    }
}