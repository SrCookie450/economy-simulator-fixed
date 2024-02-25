using Roblox.Models.Assets;

namespace Roblox.Dto.Users;

public class FeedCreatorEntry
{
    public string name { get; set; }
    public long id { get; set; }
    public string image { get; set; }
}

public class FeedEntry
{
    public CreatorType type { get; set; }
    public FeedCreatorEntry user { get; set; }
    public FeedCreatorEntry? group { get; set; }
    public long feedId { get; set; }
    public DateTime created { get; set; }
    public string content { get; set; }
}

public class UserFeedEntryDb
{
    public string username { get; set; }
    public long userId { get; set; }
    public long feedId { get; set; }
    public DateTime createdAt { get; set; }
    public string content { get; set; }
}

public class GroupFeedEntryDb : UserFeedEntryDb
{
    public long groupId { get; set; }
    public string groupName { get; set; }
    public string groupImage { get; set; }
}