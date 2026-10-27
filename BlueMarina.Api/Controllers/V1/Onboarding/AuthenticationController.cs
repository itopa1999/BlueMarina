using System.Net;
using Asp.Versioning;
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
using static BlueMarina.Application.BBL.Commands.Onboarding.Authentication.ForgotLoginPinCommand;
using static BlueMarina.Application.BBL.Commands.Onboarding.Authentication.ForgotPasswordCommand;
using static BlueMarina.Application.BBL.Commands.Onboarding.Authentication.LoginUserCommand;
using static BlueMarina.Application.BBL.Commands.Onboarding.Authentication.RefreshTokenCommand;
using static BlueMarina.Application.BBL.Commands.Onboarding.Authentication.RegisterCommand;
using static BlueMarina.Application.BBL.Commands.Onboarding.Authentication.ResendVerificationOtpCommand;
using static BlueMarina.Application.BBL.Commands.Onboarding.Authentication.VerifyAccountCommand;
using static BlueMarina.Application.BBL.Commands.Onboarding.Authentication.VerifyOtpCommand;

namespace BlueMarina.Api.Controllers.V1.Onboarding;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("1.0")]
public class AuthenticationController(
    IMediator mediator,
    IOptions<JwtSettings> jwtOptions) : BaseController
{
    private readonly IMediator _mediator = mediator;
    private readonly JwtSettings _jwt = jwtOptions.Value;

    /// <summary>
    /// Register a New User
    /// </summary>
    /// <remarks>
    /// Creates a new user account with email, phone number, and password. 
    /// Triggers an SMS OTP to the phone number and an email verification link.
    /// </remarks>
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

    /// <summary>
    /// Change Pending Phone Number
    /// </summary>
    /// <remarks>
    /// Allows a user to update their phone number before it is verified. 
    /// Invalidates old OTPs and sends a new SMS OTP to the new number.
    /// </remarks>
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

    /// <summary>
    /// Resend SMS OTP
    /// </summary>
    /// <remarks>
    /// Resends the verification OTP to the user's unverified phone number. 
    /// Subject to rate-limiting and max attempt lockout rules.
    /// </remarks>
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

    /// <summary>
    /// Resend Email Verification Link
    /// </summary>
    /// <remarks>
    /// Sends a new verification link to the authenticated user's email address. 
    /// Requires a valid JWT Bearer token.
    /// </remarks>
    [HttpPost("resend-email-verification-link")]
    [Authorize(Roles = Roles.Customer)]
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

    /// <summary>
    /// Verify Email via Link
    /// </summary>
    /// <remarks>
    /// This endpoint is hit when a user clicks the link in their email inbox.
    /// It returns a raw HTML success or failure page, not JSON, so the user can see the result in their browser.
    /// </remarks>
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
        var result = await _mediator.Send(command, cancellationToken);
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

    /// <summary>
    /// Set Login PIN
    /// </summary>
    /// <remarks>
    /// Allows an authenticated user to set their 6-digit login PIN.
    /// The PIN is hashed securely before being stored in the database.
    /// Weak PINs (e.g., 123456, 111111) will be rejected.
    /// </remarks>
    [HttpPost("set-login-pin")]
    [Authorize(Roles = Roles.Customer)]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Conflict)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> SetLoginPin(
        [FromBody] SetLoginPinCommand.Command command, 
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        
        var result = await _mediator.Send(command, cancellationToken);
        
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Verify Account via OTP
    /// </summary>
    /// <remarks>
    /// Validates the SMS OTP provided by the user. 
    /// Upon success, marks the phone as verified, consumes the OTP, and issues the first set of JWT Access and Refresh tokens.
    /// </remarks>
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

    /// <summary>
    /// User Login
    /// </summary>
    /// <remarks>
    /// Authenticates a user using email or phone number and password. 
    /// If successful and 2FA is not required, it issues JWT tokens. 
    /// If 2FA is enabled, it returns a response prompting the user to complete the 2FA step.
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(BaseResult<LoginResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> LoginUser(
        [FromBody] LoginUserCommand.Command command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(
            (int)result.StatusCode,
            result);
    }

    /// <summary>
    /// Rotate Refresh Token
    /// </summary>
    /// <remarks>
    /// Exchanges a valid Refresh Token for a new Access Token and a new Refresh Token.
    /// **Note:** The provided (old) Refresh Token is immediately revoked to prevent replay attacks (Token Rotation).
    /// If the token is expired, revoked, or invalid, it returns a 401.
    /// </remarks>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(BaseResult<RefreshTokenResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand.Command command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(
            (int)result.StatusCode,
            result
        );
    }

    /// <summary>
    /// Logout User (Revoke Session)
    /// </summary>
    /// <remarks>
    /// Invalidates the provided Refresh Token, preventing it from being used to generate new Access Tokens.
    /// Requires a valid JWT Bearer token in the Authorization header. 
    /// The current Access Token will remain valid until its natural expiration, but the user will be fully logged out once they attempt to refresh.
    /// </remarks>
    [HttpPost("logout")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutCommand.Command command, CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(
            (int)result.StatusCode,
            result
        );
    }

    /// <summary>
    /// Request Password Reset OTP
    /// </summary>
    /// <remarks>
    /// Initiates the password reset flow. If the provided email or phone exists, a 6-digit OTP is sent to both SMS and Email.
    /// </remarks>
    [HttpPost("forget-password")]
    [ProducesResponseType(typeof(BaseResult<ForgotPasswordResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ForgetPassword(
        [FromBody] ForgotPasswordCommand.Command command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(
            (int)result.StatusCode,
            result
        );
    }

    /// <summary>
    /// Request PIN Reset OTP
    /// </summary>
    /// <remarks>
    /// Initiates the PIN reset flow. Requires the user's account password for verification. 
    /// If correct, a 6-digit OTP is sent to both SMS and Email.
    /// </remarks>
    [HttpPost("forget-pin")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(BaseResult<ForgotLoginPinResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ForgetPin(
        [FromBody] ForgotLoginPinCommand.Command command, 
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Verify OTP
    /// </summary>
    /// <remarks>
    /// Verifies the OTP sent to the user's email or phone number. 
    /// Returns a ChallengeKey upon success, which must be used to reset the password or PIN.
    /// </remarks>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(BaseResult<VerifyOtpResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> VerifyOtp(
        [FromBody] VerifyOtpCommand.Command command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Reset Password
    /// </summary>
    /// <remarks>
    /// Completes the password reset flow. Requires the ChallengeKey returned from the Verify OTP step.
    /// Revokes all active sessions upon successful reset.
    /// </remarks>
    [HttpPost("reset-forgot-password")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ResetForgetPassword(
        [FromBody] ResetForgetPasswordCommand.Command command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(
            (int)result.StatusCode,
            result
        );
    }

    /// <summary>
    /// Reset Login PIN
    /// </summary>
    /// <remarks>
    /// Completes the PIN reset flow. Requires the ChallengeKey returned from the Verify OTP step.
    /// Revokes all active sessions upon successful reset.
    /// </remarks>
    [HttpPost("reset-pin")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ResetPin(
        [FromBody] ResetLoginPinCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Change Password
    /// </summary>
    /// <remarks>
    /// Allows an authenticated user to change their password by providing their old and new passwords.
    /// Revokes all active sessions upon successful change.
    /// </remarks>
    [HttpPost("change-password")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordCommand.Command command, CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(
            (int)result.StatusCode,
            result
        );
    }


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
