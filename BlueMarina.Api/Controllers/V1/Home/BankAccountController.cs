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

    /// <summary>
    /// List banks
    /// </summary>
    /// <remarks>
    /// Returns the banks available for linking a bank account, optionally filtered by name.
    /// Pass <c>?name=access</c> for a partial, case-insensitive match; omit it to return
    /// the full list. Results are cached and refreshed by the admin refresh endpoint.
    /// </remarks>
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

    /// <summary>
    /// Initiate adding a bank account
    /// </summary>
    /// <remarks>
    /// Starts the add-account flow: validates the supplied account number and bank code
    /// and prepares the account for verification. The caller must be a customer and
    /// consent to account verification. Follow up with <c>POST add-verify</c> to complete
    /// the link. The user is taken from the authenticated principal, not the request body.
    /// </remarks>
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

    /// <summary>
    /// Verify and link a bank account
    /// </summary>
    /// <remarks>
    /// Resolves the account number with the bank's gateway, persists the verified bank
    /// account, and marks it as the default if requested. Rejects duplicate accounts for
    /// the same user, invalid bank codes, and accounts that fail verification.
    /// Requires <c>Consent = true</c>. The user's id, email and first name are taken
    /// from the authenticated principal.
    /// </remarks>
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

    /// <summary>
    /// List the caller's linked bank accounts
    /// </summary>
    /// <remarks>
    /// Returns every bank account linked by the authenticated customer, including the
    /// default account flag. The user is taken from the authenticated principal.
    /// </remarks>
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

    /// <summary>
    /// Remove a linked bank account
    /// </summary>
    /// <remarks>
    /// Removes the bank account identified by <paramref name="bankAccountId"/> from the
    /// authenticated customer's linked accounts. The account must belong to the caller;
    /// attempting to remove another user's account is rejected.
    /// </remarks>
    /// <param name="bankAccountId">Id of the bank account to remove.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
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