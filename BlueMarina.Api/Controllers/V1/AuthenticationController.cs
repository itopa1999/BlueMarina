using BlueMarina.Api.Configurations;
using BlueMarina.Application.BBL.Commands.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueMarina.Api.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
public class AuthenticationController(
    IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand.Command command)
    {
        var result = await _mediator.Send(command);

        return StatusCode(
            (int)result.StatusCode,
            result);
    }

    [HttpPost("verify-account")]
    public async Task<IActionResult> VerifyEmailToken(
        [FromBody] VerifyAccountCommand.Command command)
    {
        var result = await _mediator.Send(command);

        return StatusCode(
            (int)result.StatusCode,
            result);
    }

    [HttpPost("resend-verification-token")]
    public async Task<IActionResult> ResendVerificationToken(
        [FromBody] ResendVerificationOtpCommand.Command command)
    {
        var result = await _mediator.Send(command);

        return StatusCode(
            (int)result.StatusCode,
            result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser(
        [FromBody] LoginUserCommand.Command command)
    {
        var result = await _mediator.Send(command);

        return StatusCode(
            (int)result.StatusCode,
            result);
    }

    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand.Command command)
    {
        command.Email = UserEmail;
        var result = await _mediator.Send(command);
        return StatusCode(
            (int)result.StatusCode,
            result
        );
    }


    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutCommand.Command command)
    {
        command.UserId = CurrentUserId.Value;
        var result = await _mediator.Send(command);
        return StatusCode(
            (int)result.StatusCode,
            result
        );
    }
}