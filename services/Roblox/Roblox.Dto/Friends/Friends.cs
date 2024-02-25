using Roblox.Models.Users;

namespace Roblox.Dto.Friends;

public class FriendEntry
{
    public bool isOnline { get; set; }
    public bool isDeleted { get; set; }
    public bool isBanned { get; set; }
    public long id { get; set; }
    public string name { get; set; }
    public string displayName { get; set; }
    public string? description { get; set; }
    public DateTime created { get; set; }
    public int presenceType => 0;
    public string? externalAppDisplayName => null;
    public int friendFrequentRank => 1;
}

public class FriendEntryDto
{
    public long id { get; set; }
    public string name { get; set; }
    public DateTime online_at { get; set; }
    public AccountStatus status { get; set; }
}

public class FriendRequestEntryInternal
{
    public long id { get; set; }
    public long userId { get; set; }
    public string name { get; set; }
}