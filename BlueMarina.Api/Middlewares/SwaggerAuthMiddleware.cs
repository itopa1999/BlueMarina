using System.Text;
using System.Text.Encodings.Web;

namespace BlueMarina.Api.Middlewares;

public class SwaggerAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SwaggerAuthMiddleware> _logger;

    public SwaggerAuthMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<SwaggerAuthMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            if (context.Request.Cookies.TryGetValue("SwaggerAuth", out var cookie) 
                && cookie == "authenticated")
            {
                await _next(context);
                return;
            }

            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) &&
                authHeader.ToString().StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                var encodedCredentials = authHeader.ToString().Substring("Basic ".Length).Trim();
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials)).Split(':', 2);
                var username = credentials[0];
                var password = credentials.Length > 1 ? credentials[1] : string.Empty;

                var configUsername = _configuration["SwaggerCredentials:Username"];
                var configPassword = _configuration["SwaggerCredentials:Password"];

                if (username == configUsername && password == configPassword)
                {
                    context.Response.Cookies.Append("SwaggerAuth", "authenticated", new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = context.Request.IsHttps,
                        SameSite = SameSiteMode.Strict,
                        MaxAge = TimeSpan.FromHours(1)
                    });

                    await _next(context);
                    return;
                }
            }

            context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Swagger\"";
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Authentication required to access Swagger UI.");
            return;
        }

        await _next(context);
    }
}