using System.Net;
using Asp.Versioning;
using BlueMarina.Api.Configurations;
using BlueMarina.Api.Filters;
using BlueMarina.Api.RateLimiting;
using BlueMarina.Application.BBL.Commands.BanksAccount;
using BlueMarina.Application.BBL.Queries.BanksAccount;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using static BlueMarina.Application.BBL.Commands.BanksAccount.AddBankAccount;
using static BlueMarina.Application.BBL.Commands.BanksAccount.VerifyBankAccount;
using static BlueMarina.Application.BBL.Queries.BanksAccount.GetAllBankAccount;
using static BlueMarina.Application.BBL.Queries.BanksAccount.GetBanks;

namespace BlueMarina.Api.Controllers.V1.Home;

[ApiController]
[Authorize(Roles = RolesConstants.Customer)]
[RestrictPlatform(Platforms.Mobile, Platforms.Swagger)]
[Route("api/v{version:apiVersion}/bank-account")]
[ApiVersion("1.0")]

public class BankAccountController(
    IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("list-banks")]
    [EnableRateLimiting(RateLimitPolicies.Read)]
    [ProducesResponseType(typeof(BaseResult<List<GetBanksDto>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetBanksDetailsData(
        [FromQuery] string? name, 
        CancellationToken cancellationToken)
    {
        var query = new GetBanks.Query
        {
            Name = name
        };
        var result = await _mediator.Send(query, cancellationToken);

        return StatusCode((int)result.StatusCode, result);
    }
    

    [HttpPost("iniatiate-add")]
    [EnableRateLimiting(RateLimitPolicies.Sensitive)]
    [ProducesResponseType(typeof(BaseResult<AddBankAccountDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> AddBankAccount(
        [FromBody] AddBankAccount.Command command,
        CancellationToken cancellationToken)
    {
        command.UserPublicId = UserPublicId ?? string.Empty;

        var result = await _mediator.Send(
            command,
            cancellationToken);

        return StatusCode(
            (int)result.StatusCode,
            result);
    }

    [HttpPost("add-verify")]
    [EnableRateLimiting(RateLimitPolicies.Sensitive)]
    [ProducesResponseType(typeof(BaseResult<VerifyBankAccountDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> VerifyBankAccount(
        [FromBody] VerifyBankAccount.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        command.Email = UserEmail ?? string.Empty;
        command.FirstName = UserFirstName ?? string.Empty;
        
        var result = await _mediator.Send(
            command,
            cancellationToken);

        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitPolicies.Read)]
    [ProducesResponseType(typeof(BaseResult<List<GetAllBankAccountDto>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetBankAccounts(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAllBankAccount.Query
            {
                UserId = UserPublicId ?? string.Empty
            },
            cancellationToken);

        return StatusCode((int)result.StatusCode, result);
    }


    [HttpDelete("{bankAccountId:long}/remove")]
    [EnableRateLimiting(RateLimitPolicies.Sensitive)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> RemoveBankAccounts(
        long bankAccountId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RemoveBankAccount.Command
            {
                UserId = UserPublicId ?? string.Empty,
                BankAccountId = bankAccountId
            },
            cancellationToken);

        return StatusCode((int)result.StatusCode, result);
    }
}