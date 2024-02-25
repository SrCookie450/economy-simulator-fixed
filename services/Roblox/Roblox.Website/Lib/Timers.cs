using System.Diagnostics;

namespace Roblox.Website.Lib;

public class MiddlewareTimerResult
{
    public string name { get; set; }
    public long elapsedMilliseconds { get; set; }
}

public class MiddlewareTimer
{
    public const string MiddlewareTimerKey = "MiddlewareTimer";
    private Microsoft.AspNetCore.Http.HttpContext ctx { get; set; }
    private string name { get; set; }
    private Stopwatch stopwatch { get; set; }
    public long elapsedMilliseconds => stopwatch.ElapsedMilliseconds;
    public MiddlewareTimer(Microsoft.AspNetCore.Http.HttpContext ctx, string id)
    {
        this.name = id;
        this.ctx = ctx;
        
        stopwatch = new();
        stopwatch.Start();
    }

    public void Stop()
    {
        stopwatch.Stop();
        if (!ctx.Items.ContainsKey(MiddlewareTimerKey))
        {
            ctx.Items[MiddlewareTimerKey] = new List<MiddlewareTimerResult>();
        }

        if (ctx.Items[MiddlewareTimerKey] is List<MiddlewareTimerResult> items)
            items.Add(new MiddlewareTimerResult()
            {
                elapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                name = name,
            });
        
    }
}