

using System.Net;
using BlueMarina.Api.Configurations;
using BlueMarina.Application.BBL.Commands.Kyc;
using BlueMarina.Application.BBL.Queries.Kyc;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static BlueMarina.Application.BBL.Queries.Kyc.GetKycStatusQuery;

namespace BlueMarina.Api.Controllers.V1;

[ApiController]
[Route("api/v1/kyc")]
[ApiExplorerSettings(GroupName = "v1")]

public class KycController(
    IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("get/status")]
    [ProducesResponseType(typeof(BaseResult<KycStatusDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var query = new GetKycStatusQuery.Query
        {
            UserId = CurrentUserId.Value
        };
        var result = await _mediator.Send(query, cancellationToken);

        return StatusCode(
            (int)result.StatusCode,
            result);
    }

    [HttpPost("verify/bvn")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> VerifyBvn([FromBody] VerifyKycBVN.Command command, CancellationToken cancellationToken)
    {
        command = command with { UserId = CurrentUserId.Value };
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("verify/nin")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> VerifyNin([FromBody] VerifyKycNIN.Command command, CancellationToken cancellationToken)
    {
        command = command with { UserId = CurrentUserId.Value };
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("verify/id")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> VerifyId([FromForm] VerifyKycID.Command command, CancellationToken cancellationToken)
    {
        command = command with { UserId = CurrentUserId.Value };
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }
}