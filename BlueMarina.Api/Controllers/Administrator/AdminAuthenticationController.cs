using System.Net;
using Asp.Versioning;
using BlueMarina.Api.Configurations;
using BlueMarina.Api.Filters;
using BlueMarina.Api.RateLimiting;
using BlueMarina.Application.BBL.Commands.Administrator.Authentication;
using BlueMarina.Infrastructure.Authentication.Jwt;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using static BlueMarina.Application.BBL.Commands.Administrator.Authentication.AdminLoginUserCommand;
using static BlueMarina.Application.BBL.Commands.Administrator.Authentication.AdminRefreshTokenCommand;

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
        [FromBody] AdminLoginUserCommand.Command command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess && result.Data?.Data is { } loginData)
        {
            SetAuthenticationCookies(loginData.AccessToken!, loginData.RefreshToken!);

            loginData.AccessToken = null;
            loginData.RefreshToken = null;
            loginData.AccessTokenExpiresAt = null;
        }

        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AuthMedium)]
    [ProducesResponseType(typeof(BaseResult<AdminRefreshTokenResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] AdminRefreshTokenCommand.Command command, 
        CancellationToken cancellationToken)
    {
        var cookieToken = GetRefreshTokenFromCookie();
        if (!string.IsNullOrEmpty(cookieToken))
        {
            command.RefreshToken = cookieToken;
        }

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess && result.Data is { } refreshData)
        {
            SetAuthenticationCookies(refreshData.AccessToken!, refreshData.RefreshToken!);

            refreshData.AccessToken = null;
            refreshData.RefreshToken = null;
            refreshData.AccessTokenExpiresAt = default;
        }

        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("logout")]
    [Authorize(Roles = RolesConstants.AllAdmins)]
    [EnableRateLimiting(RateLimitPolicies.AuthMedium)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Logout(
        [FromBody] AdminLogoutCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        
        var cookieToken = GetRefreshTokenFromCookie();
        if (!string.IsNullOrWhiteSpace(cookieToken))
        {
            command.RefreshToken = cookieToken;
        }

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            ClearAuthenticationCookies();
        }

        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("change-password")]
    [Authorize(Roles = RolesConstants.AllAdmins)]
    [EnableRateLimiting(RateLimitPolicies.Sensitive)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] AdminChangePasswordCommand.Command command, 
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    private string? GetRefreshTokenFromCookie()
    {
        return Request.Cookies["refresh_token"];
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
}