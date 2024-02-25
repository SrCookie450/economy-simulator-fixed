using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Roblox.Cache;
using StackExchange.Redis;

namespace Roblox.Services;

public static class Cache
{
    public static DistributedCache distributed { get; } = new();
    private static RedLockFactory? _redLock;
    public static RedLockFactory redLock
    {
        get
        {
            if (_redLock == null)
                throw new Exception("RedLock is not available");
            return _redLock;
        }
        set => _redLock = value;
    }

    public static void Configure(string connectUrl)
    {
        Roblox.Cache.DistributedCache.Configure(connectUrl);
        redLock = RedLockFactory.Create(new List<RedLockMultiplexer>()
        {
            Roblox.Cache.DistributedCache.redis,
        });
    }
}