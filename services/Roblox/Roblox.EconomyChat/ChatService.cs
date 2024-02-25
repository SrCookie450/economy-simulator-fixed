using System.Text.Json;
using Dapper;
using Roblox.Cache;
using Roblox.Dto.Users;
using Roblox.EconomyChat.Models;
using Roblox.Services;
using Roblox.Services.Exceptions;

namespace Roblox.EconomyChat;

public class ChatService : ServiceBase
{
    // Temp for proof of concept
    private static Dictionary<long, long> chatMessageToChannelId { get; set; } = new();
    private static Dictionary<long, List<ChannelChatMessage>> chatMessageDb { get; set; } = new();
    /// <summary>
    /// UserID to last read message date
    /// </summary>
    private static Dictionary<long, DateTime> lastReadDates { get; set; } = new();
    private static long lastMessageId { get; set; } = 1;
    private static Mutex messageUpdateMux = new Mutex();
    private static DistributedCache redis => Roblox.Services.Cache.distributed;

    private async Task PublishEvent<T>(T eventData) where T : IChatEvent
    {
        await redis.PublishAsync("ChatBroadcastV1", JsonSerializer.Serialize(eventData));
    }
    
    public async Task RemoveMessage(long contextUserId, long messageId)
    {
        lock (messageUpdateMux)
        {
            var channel = chatMessageToChannelId[messageId];
            chatMessageDb[channel] = chatMessageDb[channel].Where(v => v.messageId != messageId).ToList();
        }

        await PublishEvent(new ChatMessageDeletionEvent(new ChatMessageDeletionEventData()
        {
            messageId = messageId,
        }));
    }

    public async Task ToggleTyping(long contextUserId, long channelId, bool isTyping)
    {
        
    }

    public async Task<IEnumerable<Roblox.EconomyChat.Models.ChannelChatMessage>> GetMessagesInChannel(long channelId, long startMessageId, int limit)
    {
        if (!chatMessageDb.ContainsKey(channelId))
            return Array.Empty<ChannelChatMessage>();
        
        return chatMessageDb[channelId].Where(v => v.messageId > startMessageId).Take(limit);
    }

    public async Task<ChatMessage> CreateChannelMessage(long contextUserId, CreateMessageRequest request)
    {
        var channel = Channel.channels.Find(c => c.id == request.channelId);
        if (contextUserId != 12 && (channel.isAdminRequiredForReading || channel.isAdminRequiredForWriting))
            throw new RobloxException(403, 0, "Forbidden");

        var authorData = await db.QuerySingleOrDefaultAsync<UserInfo>("SELECT username FROM \"user\" u WHERE u.id = :id", new
        {
            id = contextUserId,
        });
        var msg = new ChannelChatMessage()
        {
            content = request.content,
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow,
            author = new ChatAuthor()
            {
                userId = contextUserId,
                username = authorData.username,
            },
            channelId = request.channelId,
        };
        // Db
        lock (messageUpdateMux)
        {
            msg.messageId = lastMessageId;
            chatMessageToChannelId[lastMessageId] = request.channelId;
            if (!chatMessageDb.ContainsKey(request.channelId))
                chatMessageDb[request.channelId] = new List<ChannelChatMessage>();
            chatMessageDb[request.channelId].Add(msg);
            lastMessageId++;
        }
        // Broadcast
        await PublishEvent(new ChatMessageEvent(msg));
        // Return!
        return msg;
    }

    public async Task<UnreadMessageCount> GetUnreadMessageCount(long contextUserId, long channelId)
    {
        // Default to current time so first time users don't have thousands of unread messages
        if (!lastReadDates.ContainsKey(contextUserId))
            lastReadDates[contextUserId] = DateTime.UtcNow;
        var unread = chatMessageDb[channelId].Count(c => c.createdAt > lastReadDates[contextUserId]);
        return new UnreadMessageCount()
        {
            unreadCount = unread,
        };
    }

    public async Task SetReadMessage(long contextUserId, long channelId)
    {
        lastReadDates[channelId] = DateTime.UtcNow;
    }
}