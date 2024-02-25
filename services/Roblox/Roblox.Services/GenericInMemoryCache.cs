using Roblox.Logging;

namespace Roblox.Services;

public class GenericMemoryCache<TKey,TValue> : ServiceBase, IService where TKey : notnull
{
    private Dictionary<TKey, TValue> sessionCache { get; set; } = new();
    private Object sessionCacheMux { get; set; } = new();

    public GenericMemoryCache(TimeSpan? sleepTime = null)
    {
        sleepTime ??= TimeSpan.FromMinutes(1);
        Task.Run(async () =>
        {
            Writer.Info(LogGroup.UserSessions, "Starting clear cache timer");
            while (true)
            {
                await Task.Delay(sleepTime.Value);
                lock (sessionCacheMux)
                {
                    Writer.Info(LogGroup.UserSessions, "Cleaning up cache. len = {0}", sessionCache.Count);
                    sessionCache.Clear();
                }
            }
        });
    }
    
    public Tuple<bool,TValue?> Get(TKey id)
    {
        lock (sessionCacheMux)
        {
            if (sessionCache.ContainsKey(id))
            {
                return new (true, sessionCache[id]);
            }
        }

        return new(false, default(TValue));
    }

    public void Set(TKey id, TValue entry)
    {
        lock (sessionCacheMux)
        {
            if (sessionCache.Count > 10000 || sessionCache.ContainsKey(id))
                return;
            
            sessionCache.Add(id, entry);
        }
    }

    public void Remove(TKey id)
    {
        lock (sessionCacheMux)
        {
            if (sessionCache.ContainsKey(id))
                sessionCache.Remove(id);
        }
    }
    
    public bool IsThreadSafe()
    {
        return true;
    }

    public bool IsReusable()
    {
        return true;
    }
}