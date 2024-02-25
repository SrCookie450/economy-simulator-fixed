namespace Roblox.Dto.Forums;

public class PostEntry
{
    public long postId { get; set; }
    public long userId { get; set; }
    public string username { get; set; }
    public string? post { get; set; }
    public string? title { get; set; }
    public long? threadId { get; set; }
    public long? subCategoryId { get; set; }
    public bool isPinned { get; set; }
    public bool isLocked { get; set; }
    public long views { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public class PostWithReplies
{
    public bool canDelete { get; set; }
    public bool isRead { get; set; }
    public long replyCount { get; set; }
    public PostEntry post { get; set; }
}

public class PostWithPosterInfo
{
    public bool canDelete { get; set; }
    public long postCount { get; set; }
    public DateTime createdAt { get; set; }
    public PostEntry post { get; set; }
}

public class ThreadInfo
{
    public long? nextThreadId { get; set; }
    public long? previousThreadId { get; set; }
    public long replyCount { get; set; }
    public long subCategoryId { get; set; }
    public string title { get; set; }
}

public class ForumThreadWithId
{
    /// <summary>
    /// ID of the thread parent
    /// </summary>
    public long? threadId { get; set; }
    /// <summary>
    /// ID of the post
    /// </summary>
    public long id { get; set; }
}

public class CreatePostRequest
{
    public string post { get; set; }
}

public class CreateThreadRequest
{
    public string subject { get; set; }
    public string post { get; set; }
}

public class SubCategoryData
{
    public long postCount { get; set; }
    public long threadCount { get; set; }
    public PostEntry? lastPost { get; set; }
}

public class FloodCheckPostEntry
{
    public string? subject { get; set; }
    public string? post { get; set; }
}