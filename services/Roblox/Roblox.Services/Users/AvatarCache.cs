using System.Text.Json;
using Roblox.Dto.Avatar;
using Roblox.Dto.AvatarCache;

namespace Roblox.Services;

public class AvatarCache : ServiceBase, IService
{
    private Mutex updatingMutex { get; } = new();
    private Dictionary<long, DateTime> updatedInfo { get; set; } = new();

    private string GetPendingAssetsKey(long userId)
    {
        return "AvatarCache:v1:PendingAssets:" + userId;
    }
    
    public async Task<IEnumerable<long>?> GetPendingAssets(long userId)
    {
        var key = GetPendingAssetsKey(userId);
        var result = await Cache.distributed.StringGetAsync(key);
        if (result == null) return null;
        return JsonSerializer.Deserialize<AvatarCacheAsset>(result)?.assetIds;
    }

    public async Task SetPendingAssets(long userId, IEnumerable<long> assetIds)
    {
        await Cache.distributed.StringSetAsync(GetPendingAssetsKey(userId),
            JsonSerializer.Serialize(new AvatarCacheAsset(assetIds.Distinct())), TimeSpan.FromMinutes(1));
    }

    private string GetPendingColorsKey(long userId)
    {
        return "AvatarCache:v1:PendingColors:" + userId;
    }

    public async Task<ColorEntry?> GetColors(long userId)
    {
        var key = GetPendingColorsKey(userId);
        var result = await Cache.distributed.StringGetAsync(key);
        if (result == null) return null;
        return JsonSerializer.Deserialize<ColorEntry>(result);
    }
    
    public async Task SetColors(long userId, ColorEntry colors)
    {
        await Cache.distributed.StringSetAsync(GetPendingColorsKey(userId),
            JsonSerializer.Serialize(colors), TimeSpan.FromMinutes(1));
    }

    public bool AttemptScheduleRender(long userId)
    {
        lock (updatingMutex)
        {
            if (updatedInfo.ContainsKey(userId))
                return false;
            updatedInfo[userId] = DateTime.UtcNow;
            return true;
        }
    }

    public void UnscheduleRender(long userId)
    {
        lock (updatingMutex)
        {
            if (updatedInfo.ContainsKey(userId))
                updatedInfo.Remove(userId);
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