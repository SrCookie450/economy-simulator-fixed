namespace Roblox.Dto.Assets;

public class AssetVersionEntry
{
    public long assetId { get; set; }
    public long assetVersionId { get; set; }
    public int versionNumber { get; set; }
    public long contentId { get; set; }
    public string? contentUrl { get; set; }
    /// <summary>
    /// ID of the user who uploaded the item. This is always a userId, never a groupId.
    /// </summary>
    public long creatorId { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public class AssetVersionMetadata
{
    public long assetVersionId { get; set; }
    public DateTime createdAt { get; set; }
}

public class AssetVersionMetadataImage : AssetVersionMetadata
{
    public int resolutionX { get; set; }
    public int resolutionY { get; set; }
    public int imageFormat { get; set; }
    public int sizeBytes { get; set; }
    public string hash { get; set; }
}