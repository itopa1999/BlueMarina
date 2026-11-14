using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Administrator;

public sealed class UpdateUserRolesCommand
{
    public class Command : IRequest<BaseResult>
    {
        public string UserId { get; set; } = string.Empty;

        [Required]
        public List<string> NewRoles { get; init; } = new();
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<Handler> _logger;

        public Handler(IIdentityService identityService, ILogger<Handler> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "UpdateUserRoles", ("UserPublicId", request.UserId));

            var user = await _identityService.GetByIdentifierAsync(request.UserId, cancellationToken);
            
            if (user is null)
            {
                op.Warning("User not found.");
                return new BaseResult(HttpStatusCode.NotFound, "User not found.");
            }

            var currentRoles = await _identityService.GetRolesAsync(user.Id);

            var rolesToAdd = request.NewRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToList();
            var rolesToRemove = currentRoles.Except(request.NewRoles, StringComparer.OrdinalIgnoreCase).ToList();

            if (rolesToRemove.Any())
            {
                var (removeSuccess, removeError) = await _identityService.RemoveFromRolesAsync(user.Id, rolesToRemove, cancellationToken);
                if (!removeSuccess)
                {
                    op.Warning($"Failed to remove roles: {removeError}");
                    return new BaseResult(HttpStatusCode.BadRequest, removeError);
                }
            }

            if (rolesToAdd.Any())
            {
                var (addSuccess, addError) = await _identityService.AddToRolesAsync(user.Id, rolesToAdd, cancellationToken);
                if (!addSuccess)
                {
                    op.Warning($"Failed to add roles: {addError}");
                    return new BaseResult(HttpStatusCode.BadRequest, addError);
                }
            }

            op.Success($"Roles updated for user {user.PublicId}. Added: {string.Join(",", rolesToAdd)}. Removed: {string.Join(",", rolesToRemove)}");
            
            return new BaseResult(HttpStatusCode.OK, "User roles updated successfully.");
        }
    }
}