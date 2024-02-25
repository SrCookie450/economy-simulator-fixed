using StackExchange.Redis;

namespace Roblox.Cache;

public class DistributedCache
{
    private static ConnectionMultiplexer? _redis;
    public static StackExchange.Redis.ConnectionMultiplexer redis
    {
        get
        {
            if (_redis == null)
                throw new Exception("Redis is not available");
            return _redis;
        }
        set => _redis = value;
    }
    
    private static Mutex mutex = new Mutex();
    private static Dictionary<string, Tuple<string, DateTime>> cache { get; } = new();

    public static bool IsExpired(Tuple<string, DateTime> value)
    {
        return DateTime.UtcNow > value.Item2;
    }

    public static void Configure(string connectUrl)
    {
        redis = ConnectionMultiplexer.Connect(connectUrl);
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(1));
                mutex.WaitOne();
                var toRemove = new List<string>();
                foreach (var k in cache)
                {
                    if (IsExpired(k.Value))
                    {
                        toRemove.Add(k.Key);
                    }
                }
                Console.WriteLine("[info] Roblox.Cache.DistributedCache: Removing {0} expired items", toRemove.Count);
                foreach (var old in toRemove)
                {
                    cache.Remove(old);
                }
                mutex.ReleaseMutex();
            }
        });
    }

    private static void AddToCache(string key, string value, DateTime lifetime)
    {
        if (cache.Count > 10000)
        {
            Console.WriteLine("[info] Roblox.Cache.DistributedCache.cache is too large, key will not be added: {0}",key);
            return;
        }
        cache[key] = new (value, lifetime);
    }
    
    public async Task StringSetAsync(string key, string value)
    {
        await redis.GetDatabase(0).StringSetAsync(key, value);
        
        mutex.WaitOne();
        // TODO: lower ttl if we ever use multiple servers
        AddToCache(key, value, DateTime.UtcNow.Add(TimeSpan.FromHours(1)));
        mutex.ReleaseMutex();
    }
    
    public async Task StringSetAsync(string key, string value, TimeSpan ttl)
    {
        await redis.GetDatabase(0).StringSetAsync(key, value, ttl);
        
        mutex.WaitOne();
        AddToCache(key, value, DateTime.UtcNow.Add(ttl));
        mutex.ReleaseMutex();
    }
    
    public async Task StringSetAsync(string key, long value)
    {
        await StringSetAsync(key, value.ToString());
    }

    /// <summary>
    /// Equivalent of StringGetAsync except it only checks in-memory cache - doesn't make any calls to redis
    /// </summary>
    /// <param name="key">The key to lookup in the local dictionary</param>
    /// <returns>The value if it exists, or null</returns>
    public string? StringGetMemory(string key)
    {
        mutex.WaitOne();
        if (cache.ContainsKey(key))
        {
            var exists = cache[key];
            if (!IsExpired(exists))
            {
                mutex.ReleaseMutex();
                return exists.Item1;
            }
        }

        mutex.ReleaseMutex();
        
        return null;
    }
    
    public async Task<string?> StringGetAsync(string key)
    {
        mutex.WaitOne();
        if (cache.ContainsKey(key))
        {
            var exists = cache[key];
            if (!IsExpired(exists))
            {
                mutex.ReleaseMutex();
                return exists.Item1;
            }
        }
        mutex.ReleaseMutex();
        
        var value = await redis.GetDatabase(0).StringGetAsync(key);
        if (value.HasValue)
        {
            var keyLifetime = await redis.GetDatabase(0).KeyTimeToLiveAsync(key);
            mutex.WaitOne();
            if (keyLifetime == null)
            {
                // TODO: will want to lower this if we ever add more servers.
                AddToCache(key, value!, DateTime.UtcNow.Add(TimeSpan.FromHours(1)));
            }
            else
            {
                AddToCache(key, value!, DateTime.UtcNow.Add(keyLifetime.Value));
            }
            mutex.ReleaseMutex();
        }

        return value;
    }

    public string? StringGet(string key)
    {
        return redis.GetDatabase(0).StringGet(key);
    }
    
    public void StringSet(string key, string value)
    {
        redis.GetDatabase(0).StringSet(key, value);
    }

    public async Task KeyDeleteAsync(string key)
    {
        mutex.WaitOne();
        if (cache.ContainsKey(key))
            cache.Remove(key);
        mutex.ReleaseMutex();
        await redis.GetDatabase(0).KeyDeleteAsync(key);
    }

    public async Task PublishAsync(string channel, string message)
    {
        await redis.GetDatabase(0).PublishAsync(channel, message);
    }
}