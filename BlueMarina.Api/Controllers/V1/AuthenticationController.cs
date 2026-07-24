using System.Net;
using BlueMarina.Api.Configurations;
using BlueMarina.Application.BBL.Commands.Authentication;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BlueMarina.Application.BBL.Commands.Authentication.ForgotPasswordCommand;
using static BlueMarina.Application.BBL.Commands.Authentication.LoginUserCommand;
using static BlueMarina.Application.BBL.Commands.Authentication.RefreshTokenCommand;
using static BlueMarina.Application.BBL.Commands.Authentication.RegisterCommand;
using static BlueMarina.Application.BBL.Commands.Authentication.ResendVerificationOtpCommand;
using static BlueMarina.Application.BBL.Commands.Authentication.VerifyAccountCommand;
using static BlueMarina.Application.BBL.Commands.Authentication.VerifyPasswordTokenCommand;

namespace BlueMarina.Api.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuthenticationController(
    IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("register")]
    [ProducesResponseType(typeof(BaseResult<RegistrationResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand.Command command)
    {
        var result = await _mediator.Send(command);

        return StatusCode(
            (int)result.StatusCode,
            result);
    }

    [HttpPost("verify-account")]
    [ProducesResponseType(typeof(BaseResult<VerifyAccountResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> VerifyEmailToken(
        [FromBody] VerifyAccountCommand.Command command)
    {
        var result = await _mediator.Send(command);

        return StatusCode(
            (int)result.StatusCode,
            result);
    }

    [HttpPost("resend-verification-token")]
    [ProducesResponseType(typeof(BaseResult<ResendVerificationOtpResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ResendVerificationToken(
        [FromBody] ResendVerificationOtpCommand.Command command)
    {
        var result = await _mediator.Send(command);

        return StatusCode(
            (int)result.StatusCode,
            result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(BaseResult<LoginResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
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
    [ProducesResponseType(typeof(BaseResult<RefreshTokenResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
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
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
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


    [HttpPost("forget-password")]
    [ProducesResponseType(typeof(BaseResult<ForgotPasswordResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ForgetPassword(
        [FromBody] ForgotPasswordCommand.Command command)
    {
        var result = await _mediator.Send(command);
        return StatusCode(
            (int)result.StatusCode,
            result
        );
    }


    [HttpPost("verify-forget-password")]
    [ProducesResponseType(typeof(BaseResult<VerifyPasswordTokenResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> VerifyForgetPasswordToken(
        [FromBody] VerifyPasswordTokenCommand.Command command)
    {
        var result = await _mediator.Send(command);
        return StatusCode((int)result.StatusCode, result);
    }


    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ResetForgetPassword(
        [FromBody] ResetPasswordCommand.Command command)
    {
        var result = await _mediator.Send(command);
        return StatusCode(
            (int)result.StatusCode,
            result
        );
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordCommand.Command command)
    {
        var result = await _mediator.Send(command);
        return StatusCode(
            (int)result.StatusCode,
            result
        );
    }
}