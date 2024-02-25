using System.Diagnostics;
using Roblox.Logging;

namespace Roblox.Libraries;


public class AsyncLimit
{
    private int limit { get; set; }
    private int running { get; set; } = 0;
    private List<OnCompletionJob> onCompletion { get; set; } = new();
    private Mutex completionMux = new();
    private string label { get; }
    
    public AsyncLimit(string label, int limit = 1)
    {
        this.label = label;
        this.limit = limit;
    }

    private void CallNextTask()
    {
        lock (completionMux)
        {
            Debug.Assert(running <= limit);
            if (onCompletion.Count == 0)
                return;
            
            OnCompletionJob job;
            do
            {
                Writer.Info(LogGroup.AsyncLimit, "call next job");
                job = onCompletion[0];
                onCompletion = onCompletion.Skip(1).ToList();
                if (job.cancellationToken.IsCancellationRequested)
                    continue;
                job.callback?.Invoke();
                break;
            } while (onCompletion.Count != 0);
        }
    }
    
    public Task<AsyncLimitJob> CreateAsync(TimeSpan? timeout = null)
    {
        Writer.Info(LogGroup.AsyncLimit, "CreateAsync called for {0}", label);
        timeout ??= TimeSpan.FromSeconds(60);
        
        var res = new TaskCompletionSource<AsyncLimitJob>();
        var cancelToken = new CancellationTokenSource();
        cancelToken.CancelAfter(timeout.Value);
        using var register = cancelToken.Token.Register(() =>
        {
            res.SetCanceled(cancelToken.Token);
        });
        
        var j = new AsyncLimitJob();
        j.OnFinish(() =>
        {
            lock (completionMux)
            {
                running--;
            }
            CallNextTask();
            return 0;
        });
        
        lock (completionMux)
        {
            if (running < limit)
            {
                running++;
                return Task.FromResult(j);
            }

            var onCompletionJob = new OnCompletionJob
            {
                callback = () =>
                {
                    res.SetResult(j);
                    return 0;
                },
                cancellationToken = cancelToken.Token,
            };
            onCompletion.Add(onCompletionJob);
        }

        return res.Task;
    }
}

public class OnCompletionJob
{
    public Func<int>? callback { get; set; }
    public CancellationToken cancellationToken { get; set; }
}

public class AsyncLimitJob : IAsyncDisposable
{
    private List<Func<int>> onFinish { get; set; } = new();
    
    public void OnFinish(Func<int> onFinishCb)
    {
        Writer.Info(LogGroup.AsyncLimit, "add item to onfinish callback");
        onFinish.Add(onFinishCb);
    }
    
    public ValueTask DisposeAsync()
    {
        Writer.Info(LogGroup.AsyncLimit, "calling onFinish callback");
        onFinish.ForEach(v => v());
        return ValueTask.CompletedTask;
    }
}