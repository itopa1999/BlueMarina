
using System.Net;
using BlueMarina.Api.Configurations;
using BlueMarina.Application.BBL.Queries.Profile;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BlueMarina.Application.BBL.Queries.Profile.GetProfileQuery;

namespace BlueMarina.Api.Controllers.V1;

[ApiController]
[Route("api/v1/user-profile")]
[ApiExplorerSettings(GroupName = "v1")]
public class UserProfileController(
    IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResult<GetProfileQueryDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetProfile()
    {
        var query = new GetProfileQuery.Query
        {
            UserId = CurrentUserId.Value
        };
        var result = await _mediator.Send(query);
        return StatusCode(
            (int)result.StatusCode,
            result
        );
    }

    // [HttpPut("profile")]
    // [Authorize]
    // [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    // [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    // public async Task<IActionResult> UpdateProfile(
    //     [FromBody] UpdateProfileCommand.Command command)
    // {
    //     command.UserId = CurrentUserId.Value;
    //     var result = await _mediator.Send(command);
    //     return StatusCode(
    //         (int)result.StatusCode,
    //         result
    //     );
    // }
}
