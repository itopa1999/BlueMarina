using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Shared.Logging;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Authentication;

public sealed class RefreshTokenCommand
{
    public class Command : IRequest<BaseResult<RefreshTokenResponseDto>>
    {
        [JsonIgnore]
        public string Email { get; set; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
    }

    public class RefreshTokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
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
                ("Email", request.Email)
            );

            var refreshToken = await _refreshTokenService.ValidateAsync(request.RefreshToken);

            if (refreshToken is null)
            {
                op.Fail("Invalid refresh token provided.");
                return new BaseResult<RefreshTokenResponseDto>(
                    HttpStatusCode.Unauthorized,
                    "Invalid refresh token.");
            }

            if (refreshToken.IsExpired)
            {
                op.Fail($"Refresh token expired (UserId: {refreshToken.UserId})");
                return new BaseResult<RefreshTokenResponseDto>(
                    HttpStatusCode.Unauthorized,
                    "Refresh token has expired.");
            }

            if (refreshToken.IsRevoked)
            {
                op.Fail($"Refresh token already revoked (UserId: {refreshToken.UserId})");
                return new BaseResult<RefreshTokenResponseDto>(
                    HttpStatusCode.Unauthorized,
                    "Refresh token has been revoked.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var roles = await _identityService.GetRolesAsync(refreshToken.UserId);

                var accessToken = _jwtTokenGenerator.GenerateToken(
                    refreshToken.UserId,
                    request.Email,
                    roles);

                // Revoke old refresh token
                refreshToken.RevokedAt = DateTime.UtcNow;
                _unitOfWork.Update(refreshToken);

                // Generate a new refresh token
                var (newToken, newRefreshToken) = await _refreshTokenService.CreateAsync(refreshToken.UserId);

                await _unitOfWork.AddAsync(newRefreshToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                op.Success($"Token refreshed successfully for user {refreshToken.UserId}");

                return new BaseResult<RefreshTokenResponseDto>(
                    HttpStatusCode.OK,
                    "Token refreshed successfully.",
                    new RefreshTokenResponseDto
                    {
                        AccessToken = accessToken,
                        RefreshToken = newToken,
                        AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15)
                    });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                op.Fail($"Refresh token failed for user {refreshToken.UserId}", ex);
                throw;
            }
        }
    }
}