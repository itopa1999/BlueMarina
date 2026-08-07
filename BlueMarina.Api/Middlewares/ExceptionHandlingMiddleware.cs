using System.Net;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Logging;
using BlueMarina.Shared.Results;
using Microsoft.AspNetCore.Http;

namespace BlueMarina.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.TraceIdentifier;
        context.Items["RequestId"] = requestId;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, requestId);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        string requestId)
    {
        using var op = OperationLogger.Start(
            _logger,
            "ExceptionHandling",
            ("RequestId", requestId),
            ("Path", context.Request.Path.ToString()),
            ("Method", context.Request.Method));

        if (context.Response.HasStarted)
        {
            op.Fail("Response already started", exception);
            return;
        }

        op.Fail("Unhandled exception", exception);

        context.Response.Clear();
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var result = new BaseResult(
            statusCode: HttpStatusCode.InternalServerError,
            message: "An error occurred; please try again later"
        );

        context.Response.Headers["X-Request-Id"] = requestId;

        await context.Response.WriteAsJsonAsync(result);
    }
}