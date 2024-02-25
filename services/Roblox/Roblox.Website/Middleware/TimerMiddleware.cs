using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Roblox.Website.Lib;

namespace Roblox.Website.Middleware;

public class TimerMiddleware
{
    private RequestDelegate _next;
    public TimerMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext ctx)
    {
        if (ctx.Items.ContainsKey(MiddlewareTimer.MiddlewareTimerKey))
        {
            if (ctx.Items[MiddlewareTimer.MiddlewareTimerKey] is List<MiddlewareTimerResult> results)
            {
                var strs = new List<string>();
                foreach (var item in results)
                {
                    strs.Add(item.name + "=" + item.elapsedMilliseconds);
                }

                strs.Reverse();
                ctx.Response.Headers.Add("x-timing", string.Join(",", strs));
            }
        }

        await _next(ctx);
    }
}

public static class TimerMiddlewareExtensions
{
    public static IApplicationBuilder UseTimerMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TimerMiddleware>();
    }
}