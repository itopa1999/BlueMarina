using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Helpers;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Authentication;

public sealed class ResetLoginPinCommand
{
    public class Command : IRequest<BaseResult>
    {
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string ChallengeKey { get; init; } = string.Empty;

        [Required, StringLength(6, MinimumLength = 6)]
        public string NewPin { get; init; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly SensitiveIdentifierHasher _hasher;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IUnitOfWork unitOfWork, 
            IIdentityService identityService, 
            SensitiveIdentifierHasher hasher,
            ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _hasher = hasher;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "ResetLoginPin",
                ("UserId", request.UserId)
            );

            var user = await _identityService.GetByIdentifierAsync(request.UserId, cancellationToken);
            
            if (user is null)
            {
                op.Warning($"User not found: {request.UserId}");
                return new BaseResult(HttpStatusCode.BadRequest, "User not found.");
            }

            if (!long.TryParse(request.ChallengeKey, out var otpId))
            {
                return new BaseResult(HttpStatusCode.BadRequest, "Invalid challenge key.");
            }

            var otp = await _unitOfWork.Query<OtpCode>()
                .FirstOrDefaultAsync(x => x.Id == otpId 
                                          && x.UserId == user.PublicId 
                                          && x.Purpose == OtpPurpose.PinReset, 
                                     cancellationToken);

            if (otp is null || !otp.IsUsed || otp.UsedAt == null)
            {
                op.Warning("Challenge key is invalid or OTP was not verified.");
                return new BaseResult(HttpStatusCode.BadRequest, "Invalid or expired challenge key. Please verify your OTP again.");
            }

            if (otp.UsedAt.Value < DateTimeOffset.UtcNow.AddMinutes(-15))
            {
                op.Warning("Challenge key has expired (used more than 15 minutes ago).");
                return new BaseResult(HttpStatusCode.BadRequest, "Challenge key has expired. Please request a new OTP.");
            }

            var (isWeak, errorMessage) = ExtensionHelpers.IsWeakPin(request.NewPin);
            if (isWeak)
            {
                op.Warning($"Weak PIN attempt: {errorMessage}");
                return new BaseResult(HttpStatusCode.BadRequest, errorMessage);
            }

            var newPinHash = _hasher.Hash(request.NewPin, "LOGIN_PIN");

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            var credential = await _unitOfWork.Query<Credential>()
                .FirstOrDefaultAsync(x => x.UserId == user.PublicId, cancellationToken);

            if (credential is null)
            {
                op.Warning("Credential record not found.");
                return new BaseResult(HttpStatusCode.NotFound, "User credential record not found.");
            }

            credential.LoginPinHash = newPinHash;
            _unitOfWork.Update(credential);

            var refreshTokens = await _unitOfWork.Query<RefreshToken>()
                .Where(rt => rt.UserId == user.PublicId && rt.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var token in refreshTokens)
            {
                token.RevokedAt = DateTimeOffset.UtcNow;
                token.RevocationReason = "PIN reset";
                _unitOfWork.Update(token);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            op.Success($"Login PIN reset successfully for user {user.PublicId}");
            
            return new BaseResult(HttpStatusCode.OK, "Login PIN reset successfully. Please log in with your new PIN.");
        }
    }
}