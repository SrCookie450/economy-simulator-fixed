namespace Roblox.Dto.Avatar;

public class OutfitEntry
{
    public long id { get; set; }
    public string name { get; set; }
    /// <summary>
    /// This is an extension that does not appear on modern roblox endpoints anymore. It used to though.
    /// </summary>
    public DateTime created { get; set; }
}

public class OutfitExtendedDetails
{
    public OutfitAvatar details { get; set; }
    public IEnumerable<long> assetIds { get; set; }
}