using System.Net;
using BlueMarina.Api.Configurations;
using BlueMarina.Api.RateLimiting;
using BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier1;
using BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier2;
using BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier3;
using BlueMarina.Application.BBL.Queries.Kyc;
using BlueMarina.Application.Interfaces.IdentityVerification;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using static BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier1.InitiateIdentityCheckCommand;
using static BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier2.SubmitIdDocumentCommand;
using static BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier2.SubmitSelfieCommand;
using static BlueMarina.Application.BBL.Queries.Kyc.GetLgasByStateQuery;
using static BlueMarina.Application.BBL.Queries.Kyc.GetStatesQuery;

namespace BlueMarina.Api.Controllers.V1.Onboarding;

[ApiController]
[Authorize(Roles = Roles.Customer)]
[Route("api/v1/kyc")]
[ApiExplorerSettings(GroupName = "v1")]
public class IdentityVerificationController(
    IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("tier1/identity/initiate")]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult<InitiateIdentityResponseDto>), (int)HttpStatusCode.OK)]
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

    [HttpPost("tier1/identity/verify-otp")]
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

    [HttpPost("tier1/identity/confirm")]
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

    [HttpPost("tier2/selfie/submit")]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult<SelfieCheckResponseDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> SubmitSelfie(
        [FromBody] SubmitSelfieCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("tier2/id/submit")]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult<IdDocumentResponseDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> SubmitSelfie(
        [FromBody] SubmitIdDocumentCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("tier2/signature/submit")]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Conflict)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> SubmitSignature(
        [FromBody] SubmitSignatureCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("agreements")]
    // [AllowAnonymous] todo 
    [ProducesResponseType(typeof(BaseResult<GetAgreementVersionsQuery.AgreementVersionsResponseDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAgreementVersions(CancellationToken cancellationToken)
    {
        var query = new GetAgreementVersionsQuery.Query();
        var result = await _mediator.Send(query, cancellationToken);
        
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("tier3/personal-details")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> SubmitPersonalDetails([FromBody] SubmitPersonalDetailsCommand.Command command, CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("tier3/address")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> SubmitAddress([FromBody] SubmitAddressCommand.Command command, CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("tier3/next-of-kin")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> SubmitNextOfKin([FromBody] SubmitNextOfKinCommand.Command command, CancellationToken cancellationToken)
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

    [HttpGet("locations/states")]
    [ProducesResponseType(typeof(BaseResult<List<StateDto>>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetStates(CancellationToken cancellationToken)
    {
        var query = new GetStatesQuery.Query();
        var result = await _mediator.Send(query, cancellationToken);
        
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("locations/states/{stateId:long}/lgas")]
    [ProducesResponseType(typeof(BaseResult<List<LgaDto>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetLgasByState(
        [FromRoute] long stateId,
        CancellationToken cancellationToken)
    {
        var query = new GetLgasByStateQuery.Query
        {
            StateId = stateId
        };
        
        var result = await _mediator.Send(query, cancellationToken);
        
        return StatusCode((int)result.StatusCode, result);
    }
}