namespace Roblox.EconomyChat.Models;

public enum EventType
{
    ChannelUserIsTyping,
    ChannelMessageCreated,
    ChannelMessageDeleted,
}

public interface IChatEvent
{
    
}

public class GenericEvent<T> : IChatEvent
{
    public string eventId { get; set; }
    public EventType type { get; set; }
    public T data { get; set; }

    public GenericEvent(EventType type, T data)
    {
        eventId = Guid.NewGuid().ToString();
        this.type = type;
        this.data = data;
    }
}

public class IsTypingEventData
{
    public long userId { get; set; }
    public long channelId { get; set; }
    public bool isTyping { get; set; }
    public DateTime createdAt { get; set; }
}

/// <summary>
/// User is_typing event update
/// </summary>
public class IsTypingEvent : GenericEvent<IsTypingEventData>
{
    public IsTypingEvent(IsTypingEventData data) : base(EventType.ChannelUserIsTyping, data)
    {
    }
}

/// <summary>
/// User chat message event
/// </summary>
public class ChatMessageEvent : GenericEvent<ChannelChatMessage>
{
    public ChatMessageEvent(ChannelChatMessage data) : base(EventType.ChannelMessageCreated, data)
    {
    }
}

public class ChatMessageDeletionEventData
{
    public long messageId { get; set; }
}

/// <summary>
/// Event for deletion of a chat message
/// </summary>
public class ChatMessageDeletionEvent : GenericEvent<ChatMessageDeletionEventData>
{
    public ChatMessageDeletionEvent(ChatMessageDeletionEventData data) : base(EventType.ChannelMessageDeleted, data)
    {
    }
}