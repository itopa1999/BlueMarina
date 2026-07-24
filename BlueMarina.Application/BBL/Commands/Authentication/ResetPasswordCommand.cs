using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        public Handler(IUnitOfWork unitOfWork, IIdentityService identityService)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            bool success = await _identityService.IsUserExisting(request.UserId);
            if (!success)
            {
                return new BaseResult
                (
                    HttpStatusCode.BadRequest,
                    "User not Found"
                );
            }

            var (isvalid, errorMessage) = await _identityService.ResetPasswordAsync(
                request.UserId,
                request.NewPassword);

            if (!isvalid)
                return new BaseResult(HttpStatusCode.BadRequest, errorMessage);

            var refreshTokens = await _unitOfWork.Query<RefreshToken>()
                .Where(rt => rt.UserId == request.UserId && rt.RevokedAt == null)
                .ToListAsync(cancellationToken: cancellationToken);

            foreach (var token in refreshTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevocationReason = "Password reset";
                _unitOfWork.Update(token);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new BaseResult(HttpStatusCode.OK, "Password reset successfully.");
        }
    }
}