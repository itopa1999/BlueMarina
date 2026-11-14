using System.Net;
using Asp.Versioning;
using BlueMarina.Api.Configurations;
using BlueMarina.Api.Filters;
using BlueMarina.Api.RateLimiting;
using BlueMarina.Application.BBL.Commands.Security.TwoFactor;
using BlueMarina.Application.BBL.Queries.TwoFactor;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using static BlueMarina.Application.BBL.Commands.Security.TwoFactor.SetPrimaryTwoFactorMethod;
using static BlueMarina.Application.BBL.Queries.TwoFactor.ListTwoFactorMethods;

namespace BlueMarina.Api.Controllers.V1.Security;

[ApiController]
[RestrictPlatform(Platforms.Mobile, Platforms.Swagger)]
[Authorize(Roles = RolesConstants.Customer)]
[Route("api/v{version:apiVersion}/2fa")]
[ApiVersion("1.0")]

public class TwoFactorMethodController(
    IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// List the caller's two-factor methods
    /// </summary>
    /// <remarks>
    /// Returns every 2FA method attached to the authenticated customer
    /// </remarks>
    [HttpGet("methods")]
    [EnableRateLimiting(RateLimitPolicies.Read)]
    [ProducesResponseType(typeof(BaseResult<ListTwoFactorMethodsDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> ListMethods(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ListTwoFactorMethods.Query
            {
                UserId = UserPublicId ?? string.Empty
            },
            cancellationToken);

        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Toggle the primary two-factor method
    /// </summary>
    /// <remarks>
    /// Promotes the given confirmed type to primary, demoting the current one. Calling with the
    /// type that is already primary turns it off.
    /// </remarks>
    [HttpPost("primary")]
    [EnableRateLimiting(RateLimitPolicies.Sensitive)]
    [ProducesResponseType(typeof(BaseResult<SetPrimaryTwoFactorMethodDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> SetPrimary(
        [FromBody] SetPrimaryTwoFactorMethod.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode((int)result.StatusCode, result);
    }

    
}