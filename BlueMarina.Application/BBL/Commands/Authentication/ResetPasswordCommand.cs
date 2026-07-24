using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Logging;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Authentication;

public sealed class ResetPasswordCommand
{
    public class Command : IRequest<BaseResult>
    {
        public Guid UserId { get; init; }

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string NewPassword { get; init; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly ILogger<Handler> _logger;

        public Handler(IUnitOfWork unitOfWork, IIdentityService identityService, ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "ResetPassword",
                ("UserId", request.UserId.ToString())
            );

            var userExists = await _identityService.IsUserExisting(request.UserId);
            if (!userExists)
            {
                op.Fail($"User not found: {request.UserId}");
                return new BaseResult(HttpStatusCode.BadRequest, "User not Found");
            }

            var (success, errorMessage) = await _identityService.ResetPasswordAsync(
                request.UserId,
                request.NewPassword);

            if (!success)
            {
                op.Fail($"Password reset failed for user {request.UserId}: {errorMessage}");
                return new BaseResult(HttpStatusCode.BadRequest, errorMessage);
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var refreshTokens = await _unitOfWork.Query<RefreshToken>()
                    .Where(rt => rt.UserId == request.UserId && rt.RevokedAt == null)
                    .ToListAsync(cancellationToken);

                foreach (var token in refreshTokens)
                {
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevocationReason = "Password reset";
                    _unitOfWork.Update(token);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                op.Success($"Password reset successfully for user {request.UserId}");
                return new BaseResult(HttpStatusCode.OK, "Password reset successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                op.Fail($"Password reset failed with exception for user {request.UserId}", ex);
                throw;
            }
        }
    }
}