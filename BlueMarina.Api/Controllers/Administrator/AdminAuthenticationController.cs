using System.Net;
using Asp.Versioning;
using BlueMarina.Api.Configurations;
using BlueMarina.Api.Filters;
using BlueMarina.Api.RateLimiting;
using BlueMarina.Infrastructure.Authentication.Jwt;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace BlueMarina.Api.Controllers.Administrator;

[ApiController]
[RestrictPlatform(Platforms.Web, Platforms.Swagger)]
[Route("api/v{version:apiVersion}/admin/auth")]
[ApiVersion("1.0")]
public class AdminAuthenticationController(
    IMediator mediator,
    IOptions<JwtSettings> jwtOptions) : BaseController
{
    private readonly IMediator _mediator = mediator;
    private readonly JwtSettings _jwt = jwtOptions.Value;

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult<LoginResultDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> LoginUser(
        [FromBody] LoginUserCommand.Command command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        var loginData = result.Data?.Data;

        if (result.IsSuccess && loginData is not null)
        {
            RemoveAuthenticationTokensFromResponse(
                loginData.AccessToken,
                loginData.RefreshToken,
                SetAuthenticationCookies,
                () =>
                {
                    loginData.AccessToken = null;
                    loginData.RefreshToken = null;
                    loginData.AccessTokenExpiresAt = null;
                });
        }

        return StatusCode(
            (int)result.StatusCode,
            result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AuthMedium)]
    [ProducesResponseType(typeof(BaseResult<RefreshTokenResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand.Command command, CancellationToken cancellationToken)
    {
        if (string.Equals(command.Platform, Platforms.Web, StringComparison.OrdinalIgnoreCase))
        {
            var refreshTokenFromCookie = Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(refreshTokenFromCookie))
            {
                command.RefreshToken = refreshTokenFromCookie;
            }
        }
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsSuccess && result.Data is not null)
        {
            RemoveAuthenticationTokensFromResponse(
                result.Data.AccessToken,
                result.Data.RefreshToken,
                SetAuthenticationCookies,
                () =>
                {
                    result.Data.AccessToken = null;
                    result.Data.RefreshToken = null;
                    result.Data.AccessTokenExpiresAt = null;
                });
        }

        return StatusCode(
            (int)result.StatusCode,
            result
        );
    }


    [HttpPost("logout")]
    [Authorize(Roles = RolesConstants.AllAdmins)]
    [EnableRateLimiting(RateLimitPolicies.AuthMedium)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserPublicId = UserPublicId ?? string.Empty;
        var refreshTokenFromCookie = Request.Cookies["refresh_token"];

        if (!string.IsNullOrWhiteSpace(refreshTokenFromCookie))
        {
            command.RefreshToken = refreshTokenFromCookie;
        }
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            ClearAuthenticationCookies();
        }

        return StatusCode(
            (int)result.StatusCode,
            result
        );
    }

    [HttpPost("change-password")]
    [Authorize(Roles = RolesConstants.AllAdmins)]
    [EnableRateLimiting(RateLimitPolicies.Sensitive)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordCommand.Command command, CancellationToken cancellationToken)
    {
        command.UserPublicId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(
            (int)result.StatusCode,
            result
        );
    }


    private CookieOptions CreateAuthenticationCookieOptions()
    {
        var isProduction = !HttpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment();

        var secure = isProduction || HttpContext.Request.IsHttps;

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = isProduction
                ? SameSiteMode.None
                : SameSiteMode.Strict,
            Path = "/"
        };
    }

    private void SetAuthenticationCookies(
        string accessToken,
        string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException(
                "Access token cannot be empty.",
                nameof(accessToken));

        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException(
                "Refresh token cannot be empty.",
                nameof(refreshToken));

        var accessTokenOptions = CreateAuthenticationCookieOptions();
        accessTokenOptions.Expires = DateTimeOffset.UtcNow.AddMinutes(
            _jwt.AccessTokenExpiryMinutes);
        accessTokenOptions.MaxAge = TimeSpan.FromMinutes(
            _jwt.AccessTokenExpiryMinutes);

        var refreshTokenOptions = CreateAuthenticationCookieOptions();
        refreshTokenOptions.Expires = DateTimeOffset.UtcNow.AddDays(
            _jwt.RefreshTokenExpiryDays);
        refreshTokenOptions.MaxAge = TimeSpan.FromDays(
            _jwt.RefreshTokenExpiryDays);

        Response.Cookies.Append(
            "access_token",
            accessToken,
            accessTokenOptions);

        Response.Cookies.Append(
            "refresh_token",
            refreshToken,
            refreshTokenOptions);
    }

    private void ClearAuthenticationCookies()
    {
        var accessTokenOptions = CreateAuthenticationCookieOptions();
        accessTokenOptions.Expires = DateTimeOffset.UnixEpoch;
        accessTokenOptions.MaxAge = TimeSpan.Zero;

        var refreshTokenOptions = CreateAuthenticationCookieOptions();
        refreshTokenOptions.Expires = DateTimeOffset.UnixEpoch;
        refreshTokenOptions.MaxAge = TimeSpan.Zero;

        Response.Cookies.Delete(
            "access_token",
            accessTokenOptions);

        Response.Cookies.Delete(
            "refresh_token",
            refreshTokenOptions);
    }

    private void RemoveAuthenticationTokensFromResponse(
        string? accessToken,
        string? refreshToken,
        Action<string, string> setCookies,
        Action removeTokens)
    {
        if (string.IsNullOrWhiteSpace(accessToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new InvalidOperationException(
                "Authentication tokens are required to establish web cookies.");
        }

        setCookies(accessToken, refreshToken);
        removeTokens();
    }
}