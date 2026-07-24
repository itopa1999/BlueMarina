using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Shared.Results;
using MediatR;

namespace BlueMarina.Application.BBL.Commands.Authentication;

public sealed class LogoutCommand

{
    public class Command : IRequest<BaseResult>
    {
        [JsonIgnore]
        public Guid UserId { get; set; }
        public string RefreshToken  { get; init; } = string.Empty;

    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRefreshTokenService _refreshTokenService;
        public Handler(IUnitOfWork unitOfWork, IRefreshTokenService refreshTokenService)
        {
            _unitOfWork = unitOfWork;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            
            var refreshToken =
                await _refreshTokenService.ValidateAsync(
                    request.RefreshToken);

            if (refreshToken is null)
            {
                return new BaseResult(
                    HttpStatusCode.Unauthorized,
                    "Invalid refresh token.");
            }

            if (refreshToken.IsExpired)
            {
                return new BaseResult(
                    HttpStatusCode.Unauthorized,
                    "Refresh token has expired.");
            }

            if (refreshToken.IsRevoked)
            {
                return new BaseResult(
                    HttpStatusCode.Unauthorized,
                    "Refresh token has been revoked.");
            }

            if (refreshToken.UserId != request.UserId)
                return new BaseResult(HttpStatusCode.Unauthorized, "Token does not belong to the authenticated user.");
            
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                refreshToken.RevokedAt = DateTime.UtcNow;

                _unitOfWork.Update(refreshToken);

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);
                await _unitOfWork.CommitTransactionAsync(
                    cancellationToken);

                return new BaseResult(
                    HttpStatusCode.OK,
                    "Logout Successful"
                );
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(
                    cancellationToken);
                throw;
            }
        }
    }
}