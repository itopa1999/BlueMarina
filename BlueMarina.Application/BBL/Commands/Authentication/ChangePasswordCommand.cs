

using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Application.BBL.Commands.Authentication;

public sealed class ChangePasswordCommand
{
    public class Command: IRequest<BaseResult>
    {
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

        public Handler(IUnitOfWork unitOfWork, IIdentityService identityService)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    "New password and confirmation password do not match.");
            }

            if (request.OldPassword == request.NewPassword)
            {
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    "New password cannot be the same as the old password.");
            }

            bool success = await _identityService.IsUserExisting(request.UserId);
            if (!success)
            {
                return new BaseResult
                (
                    HttpStatusCode.BadRequest,
                    "User not Found"
                );
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var (isValid, errorMessage) = await _identityService.ChangePasswordAsync(
                    request.UserId,
                    request.OldPassword,
                    request.NewPassword);

                if (!isValid)
                {
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

                return new BaseResult(
                    HttpStatusCode.OK,
                    "Password changed successfully.");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}