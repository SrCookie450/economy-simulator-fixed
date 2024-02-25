using Roblox.Models.Chat;

namespace Roblox.Dto.Chat;

public class Conversation
{
    public long id { get; set; }
    public string? title { get; set; }
    public long creatorId { get; set; }
    public ConversationType conversationType { get; set; }
    public DateTime createdAt { get; set; }
}

public class Participant
{
    public long conversationId { get; set; }
    public long userId { get; set; }
    public DateTime createdAt { get; set; }
}

public class Message
{
    /// <summary>
    /// UUID
    /// </summary>
    public string id { get; set; }
    public string message { get; set; }
    public long conversationId { get; set; }
    public long userId { get; set; }
    public DateTime createdAt { get; set; }
}

public class MessageRead
{
    public long conversationId { get; set; }
    public long userId { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime lastReadAt { get; set; }
}

public class MarkAsReadRequest
{
    public long conversationId { get; set; }
    public string endMessageId { get; set; }
}

public class StartConversationRequest
{
    public long participantUserId { get; set; }
}

public class SendMessageRequest
{
    public long conversationId { get; set; }
    public string message { get; set; }
}

public class UpdateTypingStatusRequest
{
    public long conversationId { get; set; }
    public bool isTyping { get; set; }
}

public class MessageEvent
{
    public Message message { get; set; }
    public IEnumerable<Participant> intendedRecipients { get; set; }
}
// Expected Format: [{"id":6,"title":null,"hasUnreadMessages":false,"participants":[{"type":"User","targetId":12,"name":"iHateProtogens"},{"type":"User","targetId":58,"name":"bob757"}],"conversationType":"OneToOneConversation","conversationTitle":{"titleForViewer":null,"isDefaultTitle":true},"conversationUniverse":null},
public class AddedToConversationEvent
{
    public Conversation conversation { get; set; }
    public IEnumerable<Participant> participants { get; set; }
    public IEnumerable<Participant> intendedRecipients { get; set; }
}

public class TypingEvent
{
    public long conversationId { get; set; }
    public long userId { get; set; }
    public bool isTyping { get; set; }
    public long endsAt { get; set; }
    public IEnumerable<Participant> intendedRecipients { get; set; }
}

public class EventHandler
{
    public Action<MessageEvent> onMessage { get; set; }
    public Action<AddedToConversationEvent> onAddedToConversation { get; set; }
    public Action<TypingEvent> onTyping { get; set; }
    public Action onDisconnectRequest { get; set; }
    public void Disconnect()
    {
        onDisconnectRequest();
    }
}