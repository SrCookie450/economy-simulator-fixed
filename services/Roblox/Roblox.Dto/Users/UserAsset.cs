namespace Roblox.Dto.Users;

public class CollectibleUserAssetEntry
{
    public long userAssetId { get; set; }
    public long assetId { get; set; }
    public long price { get; set; }
    public int? serial { get; set; }
    public long userId { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public class SetPriceRequest
{
    public long price { get; set; }
}

public class StaffUserAssetTrackEntry
{
    public long id { get; set; }
    public long assetId { get; set; }
    public long userId { get; set; }
    public string username { get; set; }
    public int? serial { get; set; }
}