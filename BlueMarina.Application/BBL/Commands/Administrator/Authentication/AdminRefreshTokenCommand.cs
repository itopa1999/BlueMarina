using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Administrator.Authentication;

public sealed class AdminRefreshTokenCommand
{
    public class Command : IRequest<BaseResult<AdminRefreshTokenResponseDto>>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class AdminRefreshTokenResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsAccountVerified { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTimeOffset AccessTokenExpiresAt { get; set; }
        public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult<AdminRefreshTokenResponseDto>>
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

        public async Task<BaseResult<AdminRefreshTokenResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "AdminRefreshToken",
                ("RefreshToken", request.RefreshToken)
            );

            var refreshToken = await _refreshTokenService.ValidateAsync(request.RefreshToken, cancellationToken);

            if (refreshToken is null)
            {
                op.Warning("Invalid refresh token provided.");
                return new BaseResult<AdminRefreshTokenResponseDto>(
                    HttpStatusCode.Unauthorized,
                    "Invalid refresh token.");
            }

            if (refreshToken.IsExpired)
            {
                op.Warning($"Refresh token expired (UserId: {refreshToken.UserId})");
                return new BaseResult<AdminRefreshTokenResponseDto>(
                    HttpStatusCode.Unauthorized,
                    "Refresh token has expired.");
            }

            if (refreshToken.IsRevoked)
            {
                op.Warning($"Refresh token already revoked (UserId: {refreshToken.UserId})");
                return new BaseResult<AdminRefreshTokenResponseDto>(
                    HttpStatusCode.Unauthorized,
                    "Refresh token has been revoked.");
            }

            var user = await _identityService.GetByIdentifierAsync(refreshToken.UserId, cancellationToken);

            if (user is null)
            {
                op.Warning($"User not found for refresh token (UserId: {refreshToken.UserId})");
                return new BaseResult<AdminRefreshTokenResponseDto>(
                    HttpStatusCode.Unauthorized,
                    "User not found.");
            }

            var permissions = await _identityService.GetPermissionsAsync(
                user.PublicId,
                cancellationToken);

            if (permissions is null)
            {
                op.Warning($"Admin refresh attempted on closed/soft-deleted account {user.PublicId}.");
                return new BaseResult<AdminRefreshTokenResponseDto>(
                    HttpStatusCode.Unauthorized,
                    "This account has been closed. Please contact support.");
            }

            if (!permissions.CanLogin)
            {
                op.Warning(
                    $"Admin token refresh blocked for user {user.PublicId}. " +
                    $"Status: {permissions.StatusLabel}, " +
                    $"Reason: {permissions.RestrictionReason}");

                var userMessage = string.IsNullOrWhiteSpace(permissions.StatusDescription)
                    ? "Your account is currently blocked from refreshing sessions. Please contact support."
                    : permissions.StatusDescription;

                return new BaseResult<AdminRefreshTokenResponseDto>(
                    HttpStatusCode.Unauthorized, userMessage);
            }

            var roles = await _identityService.GetRolesAsync(user.Id);

            if (roles.IsCustomer())
            {
                op.Warning($"Customer account {user.PublicId} attempted to refresh admin token.");
                return new BaseResult<AdminRefreshTokenResponseDto>(
                    HttpStatusCode.Forbidden,
                    "Customer accounts cannot access the admin portal.");
            }

            if (!roles.IsAllAdmin())
            {
                op.Warning($"User {user.PublicId} attempted admin token refresh without admin privileges.");
                return new BaseResult<AdminRefreshTokenResponseDto>(
                    HttpStatusCode.Forbidden,
                    "You do not have permission to access the admin portal.");
            }

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            refreshToken.RevokedAt = DateTimeOffset.UtcNow;
            refreshToken.RevocationReason = "Admin token refreshed";
            _unitOfWork.Update(refreshToken);

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

            var (rawRefreshToken, refreshTokenEntity) = await _refreshTokenService.CreateAsync(
                user.PublicId, cancellationToken);

            await _unitOfWork.AddAsync(refreshTokenEntity, cancellationToken);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            op.Success($"Admin token refreshed successfully for user {user.PublicId}");

            return new BaseResult<AdminRefreshTokenResponseDto>(
                HttpStatusCode.OK,
                "Token refreshed successfully.",
                new AdminRefreshTokenResponseDto
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