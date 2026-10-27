using Asp.Versioning;
using BlueMarina.Api.Configurations;
using BlueMarina.Api.Filters;
using BlueMarina.Application.BBL.Commands.Administrator;
using BlueMarina.Application.BBL.Queries.Administrator;
using BlueMarina.Infrastructure.Authentication.Jwt;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using static BlueMarina.Application.BBL.Queries.Administrator.GetAdminUserQuery;

namespace BlueMarina.Api.Controllers.Administrator;

[ApiController]
[Authorize(Roles = RolesConstants.AllAdmins)]
[RestrictPlatform(Platforms.Web, Platforms.Swagger)]
[Route("api/v{version:apiVersion}/admin/user")]
[ApiVersion("1.0")]
public class AdminController(
    IMediator mediator,
    IOptions<JwtSettings> jwtOptions) : BaseController
{
    private readonly IMediator _mediator = mediator;
    private readonly JwtSettings _jwt = jwtOptions.Value;

    /// <summary>
    /// Create a new Admin/Staff User
    /// </summary>
    /// <remarks>
    /// Allows a SuperAdmin to create a new user account and assign them specific roles (e.g., Admin, SupportAgent).
    /// </remarks>
    [HttpPost("create")]
    [Authorize(Roles = RolesConstants.SuperAdmin)]
    [ProducesResponseType(typeof(BaseResult<string>), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateAdminUserCommand.Command command, 
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Update a User's Roles
    /// </summary>
    /// <remarks>
    /// Allows a SuperAdmin to add or remove roles from an existing user. 
    /// The system will automatically figure out which roles to add and which to remove based on the provided list.
    /// Pass the target user's Public ID as a query parameter: `?userId=BM12345`.
    /// </remarks>
    [HttpPut("roles")]
    [Authorize(Roles = RolesConstants.SuperAdmin)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateRoles(
        [FromQuery] string userId,
        [FromBody] UpdateUserRolesCommand.Command command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new BaseResult(
                HttpStatusCode.BadRequest,
                "Query parameter 'userId' is required."));
        }

        command.UserId = userId;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Get a Single User
    /// </summary>
    /// <remarks>
    /// Retrieves the details of a specific user using their Public ID.
    /// Pass the target user's Public ID as a query parameter: `?userId=BM12345`.
    /// </remarks>
    [HttpGet("/roles")]
    [ProducesResponseType(typeof(BaseResult<AdminUserDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetUser(
        [FromQuery] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new BaseResult(
                HttpStatusCode.BadRequest,
                "Query parameter 'userId' is required."));
        }

        var query = new GetAdminUserQuery.Query { UserId = userId };
        var result = await _mediator.Send(query, cancellationToken);
        return StatusCode((int)result.StatusCode, result);

    }
}