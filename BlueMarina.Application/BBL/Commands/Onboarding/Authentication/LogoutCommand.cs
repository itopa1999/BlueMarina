// using System.Net;
// using System.Text.Json.Serialization;
// using BlueMarina.Application.Interfaces.Persistence;
// using BlueMarina.Application.Interfaces.Security;
// using BlueMarina.Shared.Logging;
// using BlueMarina.Shared.Results;
// using MediatR;
// using Microsoft.Extensions.Logging;

// namespace BlueMarina.Application.BBL.Commands.Authentication;

// public sealed class LogoutCommand
// {
//     public class Command : IRequest<BaseResult>
//     {
//         [JsonIgnore]
//         public string UserId { get; set; } = string.Empty;
//         public string RefreshToken { get; init; } = string.Empty;
//     }

//     public class Handler : IRequestHandler<Command, BaseResult>
//     {
//         private readonly IUnitOfWork _unitOfWork;
//         private readonly IRefreshTokenService _refreshTokenService;
//         private readonly ILogger<Handler> _logger;

//         public Handler(IUnitOfWork unitOfWork, IRefreshTokenService refreshTokenService, ILogger<Handler> logger)
//         {
//             _unitOfWork = unitOfWork;
//             _refreshTokenService = refreshTokenService;
//             _logger = logger;
//         }

//         public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
//         {
//             using var op = OperationLogger.Start(_logger, "Logout",
//                 ("UserId", request.UserId.ToString())
//             );

//             var refreshToken = await _refreshTokenService.ValidateAsync(request.RefreshToken, cancellationToken);

//             if (refreshToken is null)
//             {
//                 op.Fail("Invalid refresh token provided.");
//                 return new BaseResult(HttpStatusCode.Unauthorized, "Invalid refresh token.");
//             }

//             if (refreshToken.IsExpired)
//             {
//                 op.Fail($"Refresh token expired (UserId: {request.UserId})");
//                 return new BaseResult(HttpStatusCode.Unauthorized, "Refresh token has expired.");
//             }

//             if (refreshToken.IsRevoked)
//             {
//                 op.Fail($"Refresh token already revoked (UserId: {request.UserId})");
//                 return new BaseResult(HttpStatusCode.Unauthorized, "Refresh token has been revoked.");
//             }

//             if (refreshToken.UserProfileId != request.UserId)
//             {
//                 op.Fail($"Token owner mismatch: token belongs to {refreshToken.UserProfileId}, request user is {request.UserId}");
//                 return new BaseResult(HttpStatusCode.Unauthorized, "Token does not belong to the authenticated user.");
//             }

//             await _unitOfWork.BeginTransactionAsync(cancellationToken);

//             try
//             {
//                 refreshToken.RevokedAt = DateTime.UtcNow;
//                 _unitOfWork.Update(refreshToken);

//                 await _unitOfWork.SaveChangesAsync(cancellationToken);
//                 await _unitOfWork.CommitTransactionAsync(cancellationToken);

//                 op.Success($"User {request.UserId} logged out successfully.");
//                 return new BaseResult(HttpStatusCode.OK, "Logout Successful");
//             }
//             catch (Exception ex)
//             {
//                 await _unitOfWork.RollbackTransactionAsync(cancellationToken);
//                 op.Fail($"Logout failed for user {request.UserId}", ex);
//                 throw;
//             }
//         }
//     }
// }
