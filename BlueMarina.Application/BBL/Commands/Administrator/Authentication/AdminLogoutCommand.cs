using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Administrator.Authentication;

public sealed class AdminLogoutCommand
{
    public class Command : IRequest<BaseResult>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;
        
        [JsonIgnore]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IUnitOfWork unitOfWork, 
            IRefreshTokenService refreshTokenService, 
            ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "AdminLogout",
                ("UserId", request.UserId)
            );

            var refreshToken = await _refreshTokenService.ValidateAsync(request.RefreshToken, cancellationToken);

            if (refreshToken is null)
            {
                op.Warning("Invalid refresh token provided.");
                return new BaseResult(HttpStatusCode.Unauthorized, "Invalid refresh token.");
            }

            if (refreshToken.IsExpired)
            {
                op.Warning($"Refresh token expired (UserId: {request.UserId})");
                return new BaseResult(HttpStatusCode.Unauthorized, "Refresh token has expired.");
            }

            if (refreshToken.IsRevoked)
            {
                op.Warning($"Refresh token already revoked (UserId: {request.UserId})");
                return new BaseResult(HttpStatusCode.Unauthorized, "Refresh token has been revoked.");
            }

            if (refreshToken.UserId != request.UserId)
            {
                op.Warning($"Token owner mismatch: token belongs to {refreshToken.UserId}, request user is {request.UserId}");
                return new BaseResult(HttpStatusCode.Unauthorized, "Token does not belong to the authenticated user.");
            }

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            refreshToken.RevokedAt = DateTimeOffset.UtcNow;
            refreshToken.RevocationReason = "Admin logged out";
            _unitOfWork.Update(refreshToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            op.Success($"Admin user {request.UserId} logged out successfully.");
            
            return new BaseResult(HttpStatusCode.OK, "Logout successful.");
        }
    }
}