using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Administrator.Authentication;

public sealed class AdminChangePasswordCommand
{
    public class Command : IRequest<BaseResult>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string OldPassword { get; init; } = string.Empty;

        [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string NewPassword { get; init; } = string.Empty;

        [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string ConfirmPassword { get; init; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IUnitOfWork unitOfWork,
            IIdentityService identityService,
            ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "AdminChangePassword",
                ("UserPublicId", request.UserId));

            if (request.NewPassword != request.ConfirmPassword)
            {
                op.Warning("New password and confirmation password do not match.");
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    "New password and confirmation password do not match.");
            }

            if (request.OldPassword == request.NewPassword)
            {
                op.Warning("New password cannot be the same as old password.");
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    "New password cannot be the same as the old password.");
            }

            var user = await _identityService.GetByIdentifierAsync(request.UserId, cancellationToken);

            if (user is null)
            {
                op.Warning($"Admin user not found: {request.UserId}");
                return new BaseResult(HttpStatusCode.BadRequest, "User not found.");
            }

            var permissions = await _identityService.GetPermissionsAsync(
                user.PublicId,
                cancellationToken);

            if (permissions is null)
            {
                op.Error($"Permissions unavailable for user {user.PublicId}.");
                return new BaseResult(
                    HttpStatusCode.InternalServerError,
                    "We couldn't verify your account permissions right now. Please try again in a moment.");
            }

            if (!permissions.CanPerformSensitiveOperations)
            {
                op.Warning(
                    $"Admin password change blocked for user {user.PublicId}. " +
                    $"Status: {permissions.StatusLabel}, " +
                    $"Reason: {permissions.RestrictionReason}");

                var userMessage = string.IsNullOrWhiteSpace(permissions.StatusDescription)
                    ? "Your account is currently restricted from performing this action. Please contact support."
                    : permissions.StatusDescription;

                return new BaseResult(HttpStatusCode.Locked, userMessage);
            }

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            var (success, errorMessage) = await _identityService.ChangePasswordAsync(
                user.Id,
                request.OldPassword,
                request.NewPassword);

            if (!success)
            {
                op.Warning($"Admin password change failed: {errorMessage}");
                return new BaseResult(HttpStatusCode.BadRequest, errorMessage);
            }

            var refreshTokens = await _unitOfWork.Query<RefreshToken>()
                .Where(rt => rt.UserId == user.PublicId && rt.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var token in refreshTokens)
            {
                token.RevokedAt = DateTimeOffset.UtcNow;
                token.RevocationReason = "Admin password changed";
                _unitOfWork.Update(token);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            op.Success($"Admin password changed successfully for user {user.PublicId}");

            return new BaseResult(
                HttpStatusCode.OK,
                "Password changed successfully.");
        }
    }
}