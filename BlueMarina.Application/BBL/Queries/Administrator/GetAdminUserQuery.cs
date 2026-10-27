using MediatR;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Shared.Common.Results;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Queries.Administrator;

public sealed class GetAdminUserQuery
{
    public class Query : IRequest<BaseResult<AdminUserDto>>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class AdminUserDto
    {
        public string PublicId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }

    public class Handler : IRequestHandler<Query, BaseResult<AdminUserDto>>
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<Handler> _logger;

        public Handler(IIdentityService identityService, ILogger<Handler> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<BaseResult<AdminUserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await _identityService.GetAdminUserAsync(request.UserId, cancellationToken);
            
            if (user is null)
                return new BaseResult<AdminUserDto>(HttpStatusCode.NotFound, "User not found.");

            return new BaseResult<AdminUserDto>(
                HttpStatusCode.OK, 
                "User fetched successfully.", 
                user);
        }
    }
}