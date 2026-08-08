

using System.Net;
using BlueMarina.Api.Configurations;
using BlueMarina.Application.BBL.Queries.Kyc;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static BlueMarina.Application.BBL.Queries.Kyc.GetKycStatusQuery;

namespace BlueMarina.Api.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
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
}