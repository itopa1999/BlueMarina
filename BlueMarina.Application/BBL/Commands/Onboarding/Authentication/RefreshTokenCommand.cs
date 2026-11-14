using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Authentication;

public sealed class RefreshTokenCommand
{
    public class Command : IRequest<BaseResult<RefreshTokenResponseDto>>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsAccountVerified { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTimeOffset AccessTokenExpiresAt { get; set; }
        public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult<RefreshTokenResponseDto>>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenService refreshTokenService,
            ILogger<Handler> logger)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        public async Task<BaseResult<RefreshTokenResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "RefreshToken",
                ("RefreshToken", request.RefreshToken)
            );

            var refreshToken = await _refreshTokenService.ValidateAsync(request.RefreshToken, cancellationToken);

            if (refreshToken is null)
            {
                op.Warning("Invalid refresh token provided.");
                return new BaseResult<RefreshTokenResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid refresh token.");
            }

            if (refreshToken.IsExpired)
            {
                op.Warning($"Refresh token expired (UserId: {refreshToken.UserId})");
                return new BaseResult<RefreshTokenResponseDto>(
                    HttpStatusCode.Unauthorized,
                    "Refresh token has expired.");
            }

            if (refreshToken.IsRevoked)
            {
                op.Warning($"Refresh token already revoked (UserId: {refreshToken.UserId})");
                return new BaseResult<RefreshTokenResponseDto>(
                    HttpStatusCode.Unauthorized,
                    "Refresh token has been revoked.");
            }

            var user = await _identityService.GetByIdentifierAsync(refreshToken.UserId, cancellationToken);

            if (user is null)
            {
                op.Warning($"User not found for refresh token (UserId: {refreshToken.UserId})");
                return new BaseResult<RefreshTokenResponseDto>(
                    HttpStatusCode.NotFound,
                    "User not found.");
            }

            var permissions = await _identityService.GetPermissionsAsync(
                user.PublicId,
                cancellationToken);

            if (permissions is null)
            {
                op.Warning($"Refresh attempted on closed/soft-deleted account {user.PublicId}.");
                return new BaseResult<RefreshTokenResponseDto>(
                    HttpStatusCode.Unauthorized,
                    "This account has been closed. Please contact support.");
            }

            if (!permissions.CanLogin)
            {
                op.Warning(
                    $"Token refresh blocked for user {user.PublicId}. " +
                    $"Status: {permissions.StatusLabel}, " +
                    $"Reason: {permissions.RestrictionReason}");

                var userMessage = string.IsNullOrWhiteSpace(permissions.StatusDescription)
                    ? "Your account is currently blocked from refreshing sessions. Please contact support."
                    : permissions.StatusDescription;

                return new BaseResult<RefreshTokenResponseDto>(HttpStatusCode.Unauthorized, userMessage);
            }

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            refreshToken.RevokedAt = DateTimeOffset.UtcNow;
            refreshToken.RevocationReason = "Token refreshed";
            _unitOfWork.Update(refreshToken);

            var roles = await _identityService.GetRolesAsync(user.Id);

            var jwtResult = _jwtTokenGenerator.GenerateToken(
                user.Id,
                user.PublicId,
                user.FirstName ?? string.Empty,
                user.OtherNames ?? string.Empty,
                user.LastName ?? string.Empty,
                user.Email ?? string.Empty,
                user.PhoneNumber ?? string.Empty,
                user.FullName ?? $"{user.FirstName} {user.LastName}".Trim(),
                roles);

            var (rawRefreshToken, refreshTokenEntity) = await _refreshTokenService.CreateAsync(user.PublicId, cancellationToken);

            await _unitOfWork.AddAsync(refreshTokenEntity, cancellationToken);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            op.Success($"Token refreshed successfully for user {user.PublicId}");

            return new BaseResult<RefreshTokenResponseDto>(
                HttpStatusCode.OK,
                "Token refreshed successfully.",
                new RefreshTokenResponseDto
                {
                    UserId = user.PublicId,
                    IsAccountVerified = user.EmailConfirmed && user.PhoneNumberConfirmed,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = rawRefreshToken,
                    AccessTokenExpiresAt = jwtResult.ExpiresAt,
                    RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt
                });
        }
    }
}