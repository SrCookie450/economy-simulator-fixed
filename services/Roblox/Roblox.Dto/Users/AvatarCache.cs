namespace Roblox.Dto.AvatarCache;

public class AvatarCacheAsset
{
    public IEnumerable<long> assetIds { get; set; }
    public AvatarCacheAsset() {}

    public AvatarCacheAsset(IEnumerable<long> assets)
    {
        this.assetIds = assets;
    }
}

