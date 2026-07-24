using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Logging;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Authentication;

public sealed class ChangePasswordCommand
{
    public class Command : IRequest<BaseResult>
    {
        [JsonIgnore]
        public Guid UserId { get; set; }

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string OldPassword { get; init; } = string.Empty;

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string NewPassword { get; init; } = string.Empty;

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
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
            using var op = OperationLogger.Start(_logger, "ChangePassword",
                ("UserId", request.UserId.ToString())
            );

            if (request.NewPassword != request.ConfirmPassword)
            {
                op.Fail("New password and confirmation password do not match.");
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    "New password and confirmation password do not match.");
            }

            if (request.OldPassword == request.NewPassword)
            {
                op.Fail("New password cannot be the same as old password.");
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    "New password cannot be the same as the old password.");
            }

            var userExists = await _identityService.IsUserExisting(request.UserId);
            if (!userExists)
            {
                op.Fail($"User not found (UserId: {request.UserId})");
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    "User not found.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var (success, errorMessage) = await _identityService.ChangePasswordAsync(
                    request.UserId,
                    request.OldPassword,
                    request.NewPassword);

                if (!success)
                {
                    op.Fail($"Password change failed: {errorMessage}");
                    return new BaseResult(HttpStatusCode.BadRequest, errorMessage);
                }

                var refreshTokens = await _unitOfWork.Query<RefreshToken>()
                    .Where(rt => rt.UserId == request.UserId && rt.RevokedAt == null)
                    .ToListAsync(cancellationToken);

                foreach (var token in refreshTokens)
                {
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevocationReason = "Password changed by user";
                    _unitOfWork.Update(token);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                op.Success($"Password changed successfully for user {request.UserId}");

                return new BaseResult(
                    HttpStatusCode.OK,
                    "Password changed successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                op.Fail($"Password change failed with exception", ex);
                throw;
            }
        }
    }
}