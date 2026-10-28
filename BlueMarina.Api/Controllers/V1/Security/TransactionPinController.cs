using System.Net;
using Asp.Versioning;
using BlueMarina.Api.Configurations;
using BlueMarina.Api.Filters;
using BlueMarina.Api.RateLimiting;
using BlueMarina.Application.BBL.Commands.TransactionPin;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BlueMarina.Api.Controllers.V1.Security;

[ApiController]
[RestrictPlatform(Platforms.Mobile, Platforms.Swagger)]
[Authorize(Roles = RolesConstants.Customer)]
[Route("api/v{version:apiVersion}/security/pin")]
[ApiVersion("1.0")]
public class TransactionPinController(
    IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Set Transaction PIN
    /// </summary>
    /// <remarks>
    /// Allows an authenticated customer to set their initial 4-digit transaction PIN.
    /// 
    /// **Rules:**
    /// - The PIN must be exactly 4 digits.
    /// - A transaction PIN can only be set once. Use the "Change" endpoint to update it later.
    /// - The PIN is hashed securely using HMAC-SHA256 before storage.
    /// </remarks>
    [HttpPost("set")]
    [EnableRateLimiting(RateLimitPolicies.Sensitive)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Conflict)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Forbidden)]
    public async Task<IActionResult> SetPin(
        [FromBody] SetPinCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(
            (int)result.StatusCode,
            result);
    }

    /// <summary>
    /// Verify Transaction PIN
    /// </summary>
    /// <remarks>
    /// Verifies a customer's 4-digit transaction PIN before authorizing a sensitive financial operation.
    /// 
    /// **Security Rules:**
    /// - Lockout Policy: After 3 incorrect attempts, the user receives a final warning. After 4 incorrect attempts, the PIN is locked for 15 minutes.
    /// - On success, a short-lived `PinChallenge` is created in the database, which must be presented when executing the actual transaction.
    /// </remarks>
    [HttpPost("verify")]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.TooManyRequests)]
    public async Task<IActionResult> VerifyPin(
        [FromBody] VerifyPinCommand.Command command,
        CancellationToken cancellationToken)
    { 
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(
            (int)result.StatusCode,
            result);
    }

    /// <summary>
    /// Change Transaction PIN
    /// </summary>
    /// <remarks>
    /// Allows an authenticated customer to change their existing transaction PIN.
    /// 
    /// **Requirements:**
    /// - The user must provide their `CurrentPin` for verification.
    /// - The `NewPin` must be exactly 4 digits and cannot be the same as the current PIN.
    /// - The account must have an `Active` status (not Restricted/Suspended) to perform this sensitive operation.
    /// </remarks>
    [HttpPut("change")]
    [EnableRateLimiting(RateLimitPolicies.Sensitive)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> ChangePin(
        [FromBody] ChangePinCommand.Command command,
        CancellationToken cancellationToken)
    {    
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(
            (int)result.StatusCode,
            result);
    }



    // [HttpPost("forgot-pin-send")]
    // [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    // [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    // [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    // public async Task<IActionResult> SendOtpForPinForgot(
    //     CancellationToken cancellationToken)
    // {
    //     var result = await _mediator.Send(
    //         new SendForgotPinOtpCommand.Command
    //         {
    //             UserPublicId = UserPublicId ?? string.Empty
    //         },
    //         cancellationToken);

    //     return StatusCode(
    //         (int)result.StatusCode,
    //         result);
    // }

    // [HttpPost("forgot-pin-verify")]
    // [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    // [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    // [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    // public async Task<IActionResult> VerifyOtpForPinForgot(
    //     [FromBody] VerifyForgotPinOtpCommand.Command command,
    //     CancellationToken cancellationToken)
    // {
    //     command.UserPublicId = UserPublicId ?? string.Empty;
    //     command.UserId = CurrentUserId ?? 0;

    //     var result = await _mediator.Send(command, cancellationToken);

    //     return StatusCode(
    //         (int)result.StatusCode,
    //         result);
    // }
}