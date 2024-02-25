using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Roblox.Models.Sessions;
using Roblox.Models.Staff;

namespace Roblox.Website.Filters;

public class HttpCacheFilter : ActionFilterAttribute, IAsyncActionFilter
{
    private int durationInSeconds { get; set; }
    private bool isPublic { get; set; }
    
    public HttpCacheFilter(int durationInSeconds, bool isPublic)
    {
        this.durationInSeconds = durationInSeconds;
        this.isPublic = isPublic;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cache = new CacheControlHeaderValue()
        {
            MaxAge = TimeSpan.FromSeconds(durationInSeconds),
            Public = isPublic,
            Private = !isPublic,
            // SharedMaxAge = TimeSpan.FromSeconds(durationInSeconds),
        };
        context.HttpContext.Response.Headers.CacheControl = cache.ToString();
        await next();
    }
}