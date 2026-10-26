using BlueMarina.Shared.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BlueMarina.Api.Configurations;

public class ApiVersionResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult && 
            objectResult.Value is BaseResult baseResult)
        {
            var version = context.HttpContext.GetRequestedApiVersion();
            if (version != null)
            {
                baseResult.Version = $"v{version.MajorVersion}";
            }

            if (context.HttpContext.Items.TryGetValue("Platform", out var platform) && platform is string platformStr)
            {
                baseResult.Platform = platformStr;
            }
            else
            {
                baseResult.Platform = "Unknown";
            }
        }
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}