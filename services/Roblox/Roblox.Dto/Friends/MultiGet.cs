namespace Roblox.Dto.Friends;

public class MultiGetStatusEntry
{
    public long id { get; set; }
    public string status { get; set; }
}

public class FollowingExistsRequest
{
    public IEnumerable<long> targetUserIds { get; set; }
}