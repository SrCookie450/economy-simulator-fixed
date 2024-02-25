namespace Roblox.Dto.Staff;

public class ResolveThumbAssetEntry
{
    public long assetId { get; set; }
}

public class ResolveThumbUsersEntry
{
    public long userId { get; set; }
}

public class ResolveThumbGroupsEntry
{
    public long groupId { get; set; }
}

public class StaffAssetResolveThumbnailResponse
{
    public IEnumerable<ResolveThumbAssetEntry> assets { get; set; } = ArraySegment<ResolveThumbAssetEntry>.Empty;
    public IEnumerable<ResolveThumbUsersEntry> users { get; set; } = ArraySegment<ResolveThumbUsersEntry>.Empty;
    public IEnumerable<ResolveThumbGroupsEntry> groups { get; set; } = ArraySegment<ResolveThumbGroupsEntry>.Empty;
}