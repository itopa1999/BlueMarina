using System.Net;
using BlueMarina.Api.Configurations;
using BlueMarina.Api.RateLimiting;
using BlueMarina.Application.BBL.Commands.Onboarding.Authentication;
using BlueMarina.Infrastructure.Authentication.Jwt;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using static BlueMarina.Application.BBL.Commands.Onboarding.Authentication.ChangePendingPhoneCommand;
using static BlueMarina.Application.BBL.Commands.Onboarding.Authentication.RegisterCommand;
using static BlueMarina.Application.BBL.Commands.Onboarding.Authentication.ResendVerificationOtpCommand;
using static BlueMarina.Application.BBL.Commands.Onboarding.Authentication.VerifyAccountCommand;

namespace BlueMarina.Api.Controllers.V1.Onboarding;

[ApiController]
[Route("api/v1/auth")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuthenticationController(
    IMediator mediator,
    IOptions<JwtSettings> jwtOptions) : BaseController
{
    private readonly IMediator _mediator = mediator;
    private readonly JwtSettings _jwt = jwtOptions.Value;

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult<RegistrationResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand.Command command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(
            (int)result.StatusCode,
            result);
    }

    [HttpPost("change-pending-phone")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult<ChangePendingPhoneResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Conflict)]
    public async Task<IActionResult> ChangePendingPhone(
        [FromBody] ChangePendingPhoneCommand.Command command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("resend-otp-sms")]
    [ProducesResponseType(typeof(BaseResult<ResendVerificationOtpResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ResendVerificationOtp(
        [FromBody] ResendVerificationOtpCommand.Command command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(
            (int)result.StatusCode,
            result);
    }


    [HttpPost("resend-email-verification-link")]
    [Authorize (Roles = Roles.Customer)]
    [ProducesResponseType(typeof(BaseResult<ResendVerificationOtpResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ResendEmailVerificationLink(
        CancellationToken cancellationToken)
    {
        var command = new ResendEmailVerificationLinkCommand.Command
        {
            UserId = UserPublicId ?? string.Empty
        };
        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(
            (int)result.StatusCode,
            result);
    }

    [HttpGet("verify-email")]
    [AllowAnonymous]
    [Produces("text/html")]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] string userId,
        [FromQuery] string code,
        CancellationToken cancellationToken)
    {
        var command = new VerifyEmailCommand.Command 
        { 
            UserId = userId, 
            Code = code 
        };
        var result = await _mediator.Send( command, cancellationToken);
        if (result.StatusCode == HttpStatusCode.OK)
        {
            return Content(
                "<html><body style='font-family:sans-serif;text-align:center;padding:60px;'>" +
                "<h1 style='color:#1A3A6B;'>Email Verified</h1>" +
                "<p style='color:#3E4F66;'>Your email address has been confirmed. You can close this tab.</p>" +
                "</body></html>",
                "text/html");
        }

        return Content(
            "<html><body style='font-family:sans-serif;text-align:center;padding:60px;'>" +
            "<h1 style='color:#C0392B;'>Verification Failed</h1>" +
            $"<p style='color:#3E4F66;'>{System.Net.WebUtility.HtmlEncode(result.Message)}</p>" +
            "</body></html>",
            "text/html");
    }

    [HttpPost("verify-account")]
    [ProducesResponseType(typeof(BaseResult<VerifyAccountResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> VerifyEmailToken(
        [FromBody] VerifyAccountCommand.Command command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(
            (int)result.StatusCode,
            result);
    }

//     [HttpPost("login")]
//     [ProducesResponseType(typeof(BaseResult<LoginResponseDto>), (int)HttpStatusCode.OK)]
//     [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
//     public async Task<IActionResult> LoginUser(
//         [FromBody] LoginUserCommand.Command command, CancellationToken cancellationToken)
//     {
//         var result = await _mediator.Send(command, cancellationToken);

//         if (result.IsSuccess &&
//             result.Data is not null &&
//             // !result.Data.Requires2Fa &&
//             string.Equals(result.Data.Platform, Platforms.Web, StringComparison.OrdinalIgnoreCase))
//         {
//             SetAuthenticationCookies(
//                 result.Data.AccessToken,
//                 result.Data.RefreshToken);
//         }

//         return StatusCode(
//             (int)result.StatusCode,
//             result);
//     }

//     [HttpPost("refresh-token")]
//     [ProducesResponseType(typeof(BaseResult<RefreshTokenResponseDto>), (int)HttpStatusCode.OK)]
//     [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
//     public async Task<IActionResult> RefreshToken(
//         [FromBody] RefreshTokenCommand.Command command, CancellationToken cancellationToken)
//     {
//         if (string.Equals(command.Platform, Platforms.Web, StringComparison.OrdinalIgnoreCase))
//         {
//             var refreshTokenFromCookie = Request.Cookies["refresh_token"];
//             if (!string.IsNullOrEmpty(refreshTokenFromCookie))
//             {
//                 command.RefreshToken = refreshTokenFromCookie;
//             }
//         }
//         var result = await _mediator.Send(command, cancellationToken);

//         var isWebPlatform = string.Equals(command.Platform, Platforms.Web, StringComparison.OrdinalIgnoreCase);
//         if (isWebPlatform)
//         {
//             if (result.IsSuccess && result.Data is not null)
//             {
//                 SetAuthenticationCookies(
//                     result.Data.AccessToken,
//                     result.Data.RefreshToken);
//             }
//             else
//             {
//                 ClearAuthenticationCookies();
//             }
//         }
//         return StatusCode(
//             (int)result.StatusCode,
//             result
//         );
//     }


//     [HttpPost("logout")]
//     [Authorize]
//     [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
//     [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
//     public async Task<IActionResult> Logout(
//         [FromBody] LogoutCommand.Command command, CancellationToken cancellationToken)
//     {
//         command.UserId = CurrentUserId.Value;
//         var result = await _mediator.Send(command, cancellationToken);
//         return StatusCode(
//             (int)result.StatusCode,
//             result
//         );
//     }


//     [HttpPost("forget-password")]
//     [ProducesResponseType(typeof(BaseResult<ForgotPasswordResponseDto>), (int)HttpStatusCode.OK)]
//     [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
//     public async Task<IActionResult> ForgetPassword(
//         [FromBody] ForgotPasswordCommand.Command command, CancellationToken cancellationToken)
//     {
//         var result = await _mediator.Send(command, cancellationToken);
//         return StatusCode(
//             (int)result.StatusCode,
//             result
//         );
//     }


//     [HttpPost("verify-forget-password")]
//     [ProducesResponseType(typeof(BaseResult<VerifyPasswordTokenResponseDto>), (int)HttpStatusCode.OK)]
//     [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
//     public async Task<IActionResult> VerifyForgetPasswordToken(
//         [FromBody] VerifyPasswordTokenCommand.Command command, CancellationToken cancellationToken)
//     {
//         var result = await _mediator.Send(command, cancellationToken);
//         return StatusCode((int)result.StatusCode, result);
//     }


//     [HttpPost("reset-password")]
//     [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
//     [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
//     public async Task<IActionResult> ResetForgetPassword(
//         [FromBody] ResetPasswordCommand.Command command, CancellationToken cancellationToken)
//     {
//         var result = await _mediator.Send(command, cancellationToken);
//         return StatusCode(
//             (int)result.StatusCode,
//             result
//         );
//     }

//     [HttpPost("change-password")]
//     [Authorize]
//     [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
//     [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
//     public async Task<IActionResult> ChangePassword(
//         [FromBody] ChangePasswordCommand.Command command, CancellationToken cancellationToken)
//     {
//         command.UserId = CurrentUserId.Value;
//         var result = await _mediator.Send(command, cancellationToken);
//         return StatusCode(
//             (int)result.StatusCode,
//             result
//         );
//     }

//     [HttpPost("referral/apply")]
//     [Authorize]
//     [ProducesResponseType(typeof(BaseResult<ApplyReferralResponseDto>), (int)HttpStatusCode.OK)]
//     [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
//     public async Task<IActionResult> ApplyReferralCode(
//         [FromBody] ApplyReferralCommand.Command command, CancellationToken cancellationToken)
//     {
//         command.UserId = CurrentUserId.Value;
//         var result = await _mediator.Send(command, cancellationToken);
//         return StatusCode(
//             (int)result.StatusCode,
//             result
//         );
//     }

//     private void SetAuthenticationCookies(
//         string accessToken,
//         string refreshToken)
//     {

//         var accessTokenOptions = new CookieOptions
//         {
//             HttpOnly = true,
//             Secure = HttpContext.Request.IsHttps,
//             SameSite = SameSiteMode.Strict,
//             Path = "/",
//             Expires = DateTimeOffset.UtcNow.AddMinutes(
//                 _jwt.AccessTokenExpiryMinutes),
//             MaxAge = TimeSpan.FromMinutes(_jwt.AccessTokenExpiryMinutes)
//         };

//         Response.Cookies.Append("access_token", accessToken, accessTokenOptions);

//         var refreshTokenOptions = new CookieOptions
//         {
//             HttpOnly = true,
//             Secure = HttpContext.Request.IsHttps,
//             SameSite = SameSiteMode.Strict,
//             Path = "/",
//             Expires = DateTimeOffset.UtcNow.AddDays(
//                 _jwt.RefreshTokenExpiryDays),
//             MaxAge = TimeSpan.FromDays(_jwt.RefreshTokenExpiryDays)
//         };

//         Response.Cookies.Append("refresh_token", refreshToken, refreshTokenOptions);

//     }
//     private void ClearAuthenticationCookies()
//     {
//         var cookieOptions = new CookieOptions
//         {
//             HttpOnly = true,
//             Secure = HttpContext.Request.IsHttps,
//             SameSite = SameSiteMode.Strict,
//             Path = "/",
//             Expires = DateTimeOffset.UtcNow.AddYears(-1),
//             MaxAge = TimeSpan.Zero
//         };

//         Response.Cookies.Append("access_token", string.Empty, cookieOptions);
//         Response.Cookies.Append("refresh_token", string.Empty, cookieOptions);
//     }
}
