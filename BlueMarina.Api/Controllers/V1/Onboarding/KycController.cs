using System.Net;
using BlueMarina.Api.Configurations;
using BlueMarina.Api.RateLimiting;
using BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier1;
using BlueMarina.Application.BBL.Queries.Kyc;
using BlueMarina.Application.Interfaces.IdentityVerification;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BlueMarina.Api.Controllers.V1.Onboarding;

[ApiController]
[Authorize(Roles = Roles.Customer)]
[Route("api/v1/kyc")]
[ApiExplorerSettings(GroupName = "v1")]
public class IdentityVerificationController(
    IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("identity-check/initiate")]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult<InitiateIdentityCheckCommand.InitiateIdentityResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Conflict)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.ServiceUnavailable)]
    public async Task<IActionResult> InitiateIdentityCheck(
        [FromBody] InitiateIdentityCheckCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("identity-check/verify-otp")]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult<BvnDetailsDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.TooManyRequests)]
    public async Task<IActionResult> VerifyIdentityOtp(
        [FromBody] VerifyIdentityOtpCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;

        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("identity-check/confirm")]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> ConfirmIdentityDetails(
        [FromBody] ConfirmIdentityDetailsCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;

        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("progress")]
    [ProducesResponseType(typeof(BaseResult<GetOnboardingProgressQuery.OnboardingProgressDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetOnboardingProgress(CancellationToken cancellationToken)
    {
        var query = new GetOnboardingProgressQuery.Query
        {
            UserId = UserPublicId ?? string.Empty
        };
        
        var result = await _mediator.Send(query, cancellationToken);
        
        return StatusCode((int)result.StatusCode, result);
    }
}