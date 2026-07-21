using System.Net;
using BlueMarina.Shared.Constants;
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
        if (context.Response.HasStarted)
        {
            _logger.LogError(exception,
                "Response already started | RequestId: {RequestId}",
                requestId);

            return;
        }

        _logger.LogError(exception,
            "Unhandled Exception | RequestId: {RequestId} | Path: {Path} | Method: {Method}",
            requestId,
            context.Request.Path,
            context.Request.Method);

        context.Response.Clear();
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var result = new BaseResult(
            statusCode: HttpStatusCode.InternalServerError,
            message: "An error occurred; please try again later",
            errorCode: ErrorCode.INTERNAL_SERVER_ERROR
        );

        context.Response.Headers["X-Request-Id"] = requestId;

        await context.Response.WriteAsJsonAsync(result);
    }
}