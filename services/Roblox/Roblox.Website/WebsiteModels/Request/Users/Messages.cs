namespace Roblox.Website.WebsiteModels.Users;

public class SendMessageRequest
{
    public long recipientid { get; set; }
    public string body { get; set; }
    public string subject { get; set; }
    public long? replyMessageId { get; set; }
    public bool includePreviousMessage { get; set; }
}

public class MultiUpdateMessagesRequest
{
    public IEnumerable<long> messageIds { get; set; }
}