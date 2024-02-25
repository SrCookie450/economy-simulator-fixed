using System.Text.RegularExpressions;
using Dapper;
using Newtonsoft.Json;
using Roblox.Dto.Chat;
using Roblox.Logging;
using Roblox.Services.Exceptions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Roblox.Services;

public class ChatService : ServiceBase, IService
{
    public async Task<IEnumerable<Conversation>> GetUserConversations(long userId)
    {
        return await db.QueryAsync<Conversation>("SELECT p.conversation_id as id, c.creator_id as userId, c.conversation_type as conversationType, c.created_at as createdAt FROM user_conversation_participant p INNER JOIN user_conversation c ON c.id = p.conversation_id WHERE p.user_id = @userId ORDER BY p.conversation_id desc", new
        {
            userId,
        });
    }
    
    public async Task<IEnumerable<Participant>> GetChatParticipants(long conversationId)
    {
        return await db.QueryAsync<Participant>("SELECT user_id as userId, conversation_id as conversationId, created_at as createdAt FROM user_conversation_participant WHERE conversation_id = @conversationId", new
        {
            conversationId,
        });
    }

    public async Task<Message?> GetLatestMessageInConversation(long conversationId)
    {
        return await db.QueryFirstOrDefaultAsync<Message?>("SELECT id, conversation_id as conversationId, user_id as userId, message, created_at as createdAt FROM user_conversation_message WHERE conversation_id = @conversationId ORDER BY created_at desc LIMIT 1", new
        {
            conversationId,
        });
    }
    
    public async Task<IEnumerable<Message>> GetLatestMessagesInConversation(long conversationId, string exclusiveStartId, int limit)
    {
        return await db.QueryAsync<Message>("SELECT id, conversation_id as conversationId, user_id as userId, message, created_at as createdAt FROM user_conversation_message WHERE conversation_id = @conversationId ORDER BY created_at desc LIMIT :limit", new
        {
            conversationId,
            limit,
        });
    }

    public async Task<bool> DoesHaveUnreadMessages(long conversationId, long contextUserId)
    {
        var latestMessage = await db.QuerySingleOrDefaultAsync<Message>(
            "SELECT created_at as createdAt FROM user_conversation_message WHERE conversation_id = :id ORDER BY created_at DESC LIMIT 1", new
            {
                id = conversationId,
            });
        var userHasRead = await db.QuerySingleOrDefaultAsync<MessageRead>("SELECT updated_at as lastReadAt FROM user_conversation_message_read WHERE user_id = :user_id AND conversation_id = :conversation_id", new
        {
            user_id = contextUserId,
            conversation_id = conversationId,
        });
        return latestMessage != null && (userHasRead == null || latestMessage.createdAt > userHasRead.lastReadAt);
    }

    public async Task<Message?> GetMessageById(string messageId)
    {
        return await db.QuerySingleOrDefaultAsync<Message?>("SELECT id, conversation_id as conversationId, user_id as userId, created_at as createdAt, message FROM user_conversation_message WHERE id = :id",
            new
            {
                id = messageId,
            });
    }

    public async Task<bool> IsUserInConversation(long conversationId, long userId)
    {
        var result = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM user_conversation_participant WHERE user_id = :user_id AND conversation_id = :conversation_id", new
            {
                user_id = userId,
                conversation_id = conversationId,
            });
        return result.total > 0;
    }

    public async Task<bool> IsRead(string messageId, long conversationId, long userId)
    {
        var msg = await GetMessageById(messageId);
        if (msg == null) return false; // ?
        
        var result = await db.QuerySingleOrDefaultAsync<MessageRead>(
            "SELECT updated_at as lastReadAt FROM user_conversation_message_read WHERE user_id = :user_id AND conversation_id = :conversation_id",
            new
            {
                user_id = userId,
                conversation_id = conversationId,
            });
        if (result == null)
            return false;
        
        return msg.createdAt <= result.lastReadAt;
    }
    
    public async Task MarkMessageAsRead(long conversationId, string messageId, long userId)
    {
        var data = await GetMessageById(messageId);
        if (data == null || data.conversationId != conversationId || !await IsUserInConversation(conversationId, userId))
            throw new RobloxException(403, 3,
                "Failed to send GRPC request.\r\n\tService: roblox.chat.chatgateway.v1.ChatGatewayAPI\r\n\tMethod: MarkMessageAsRead\r\n\tHost: 10.0.27.165:22105\r\n\tStatus: PermissionDenied (User is not part of the conversation. UserId: "+userId+". ConversationId: "+conversationId+". Context: None.)\r\n");
        await db.ExecuteAsync(
            "INSERT INTO user_conversation_message_read (conversation_id, user_id, created_at, updated_at) VALUES (:conversation_id, :user_id, :created_at, :created_at) ON CONFLICT (conversation_id, user_id) DO UPDATE SET updated_at = :created_at",
            new
            {
                conversation_id = conversationId,
                user_id = userId,
                created_at = DateTime.UtcNow,
            });
    }

    public async Task<IEnumerable<Conversation>> GetAllConversationsInitiatedByUser(long userId)
    {
        var result = await db.QueryAsync<Conversation>("SELECT id, created_at as createdAt, creator_id as creatorId, title, conversation_type as conversationType FROM user_conversation WHERE creator_id = :user_id",
            new
            {
                user_id = userId,
            });
        return result;
    }

    private string GetCreateConvoLock(long userIdA, long userIdB)
    {
        if (userIdA > userIdB)
            return "chat_convo_" + userIdB + "_" + userIdA;
        return "chat_convo_" + userIdA + "_" + userIdB;
    }

    public async Task AddUserToConversation(long conversationId, long userId)
    {
        await db.ExecuteAsync("INSERT INTO user_conversation_participant (user_id, conversation_id) VALUES (:user_id, :conversation_id) ON CONFLICT (user_id, conversation_id) DO NOTHING",
            new
            {
                user_id = userId,
                conversation_id = conversationId,
            });
    }
    
    public async Task<Conversation> CreateOneToOneConversation(long userIdInitiating, long userId)
    {
        using var friends = ServiceProvider.GetOrCreate<FriendsService>(this);
        var areFriends = await friends.AreAlreadyFriends(userIdInitiating, userId);
        if (!areFriends)
            throw new RobloxException(403, 0, "Forbidden");
        
        var dupeLockKey = GetCreateConvoLock(userIdInitiating, userId);
        await using var redlock = await Services.Cache.redLock.CreateLockAsync(dupeLockKey, TimeSpan.FromSeconds(5));
        if (!redlock.IsAcquired)
            throw new RobloxException(400, 0, "Failed to acquire lock for conversation creation.");
        
        // check if it already exists
        var first = await GetAllConversationsInitiatedByUser(userIdInitiating);
        foreach (var item in first)
        {
            var participants = (await GetChatParticipants(item.id)).ToArray();
            if (participants.Any((p) => p.userId == userId))
            {
                return item;
            }
        }
        var second = await GetAllConversationsInitiatedByUser(userId);
        foreach (var item in second)
        {
            var participants = (await GetChatParticipants(item.id)).ToArray();
            if (participants.Any((p) => p.userId == userIdInitiating))
            {
                return item;
            }
        }
        // doesn't exist, so we can create it.
        return await InTransaction(async (trx) =>
        {
            var result = await db.QuerySingleOrDefaultAsync<Conversation>("INSERT INTO user_conversation (creator_id, conversation_type) VALUES (:creator_id, :conversation_type) RETURNING id, created_at as createdAt, creator_id as creatorId, title, conversation_type as conversationType",
                new
                {
                    creator_id = userIdInitiating,
                    conversation_type = 1,
                });

            await AddUserToConversation(result.id, userIdInitiating);
            await AddUserToConversation(result.id, userId);
            
            return result;
        });
    }

    private const string redisMessagesChannel = "RobloxChatMessagesV1";
    private const string redisAddedToConversationChannel = "RobloxChatAddedToConversationV1";
    private const string redisRemovedFromConversationChannel = "RobloxChatRemovedFromConversationV1";
    private const string redisTypingChannel = "RobloxChatTypingV1";

    private static async Task GenericHandler<T>(Action<T> handler, string channel)
    {
        var connection = await Roblox.Cache.DistributedCache.redis.GetSubscriber().SubscribeAsync(channel);
        connection.OnMessage(msg =>
        {
            if (msg.Channel == channel && !string.IsNullOrWhiteSpace(msg.Message))
            {
                try
                {
                    var data = JsonSerializer.Deserialize<T>(msg.Message);
                    if (data != null)
                    {
                        handler(data);
                    }
                }
                catch (Exception e)
                {
                    Writer.Info(LogGroup.RealTimeChat, "Error deserializing chat message: {0}", e.Message);
                }
            }
        });
    }
    
    private static async Task ListenForMessagesFromRedis(Action<MessageEvent> handler, Action<AddedToConversationEvent> addedToConversation, Action<TypingEvent> typing)
    {
        await GenericHandler(handler, redisMessagesChannel);
        await GenericHandler(addedToConversation, redisAddedToConversationChannel);
        await GenericHandler(typing, redisTypingChannel);
    }

    private static object messageMux { get; set; } = new();
    private static List<Roblox.Dto.Chat.EventHandler>? messageHandlers { get; set; } = null;
    private static async Task<Roblox.Dto.Chat.EventHandler> ListenForMessages(Roblox.Dto.Chat.EventHandler handler)
    {
        var ev = new Roblox.Dto.Chat.EventHandler
        {
            onDisconnectRequest = () =>
            {
                lock (messageMux)
                {
                    messageHandlers?.Remove(handler);
                }
            }
        };
        var doCreate = false;
        lock (messageMux)
        {
            if (messageHandlers != null)
            {
                Console.WriteLine("[info] Using existing messageHandler instead of creating one");
                messageHandlers.Add(handler);
            }
            else
            {
                Console.WriteLine("[info] WIll create a message handler");
                messageHandlers = new() {handler};
                doCreate = true;
            }
        }

        if (doCreate)
        {
            Console.WriteLine("[info] ListenForMessagesFromRedis");
            await ListenForMessagesFromRedis(msg =>
            {
                List<Roblox.Dto.Chat.EventHandler>? handlers = null;
                lock (messageMux)
                {
                    handlers = messageHandlers;
                }
                
                foreach (var item in handlers!)
                {
                    item.onMessage(msg);
                }
            }, added =>
            {
                List<Roblox.Dto.Chat.EventHandler>? handlers = null;
                lock (messageMux)
                {
                    handlers = messageHandlers;
                }
                
                foreach (var item in handlers!)
                {
                    item.onAddedToConversation(added);
                }
            }, typing =>
            {
                List<Roblox.Dto.Chat.EventHandler>? handlers = null;
                lock (messageMux)
                {
                    handlers = messageHandlers;
                }
                
                foreach (var item in handlers!)
                {
                    item.onTyping(typing);
                }
            });
        }

        return ev;
    }
    
    public async Task<Roblox.Dto.Chat.EventHandler> ListenForMessages(long userId, Action<MessageEvent> messageHandler, Action<AddedToConversationEvent> addedHandler, Action<TypingEvent> typingHandler)
    {
        var ev = new Roblox.Dto.Chat.EventHandler();
        ev.onMessage = msg =>
        {
            var intended = msg.intendedRecipients.ToArray();
            if (intended.Any(a => a.userId == userId))
            {
                messageHandler(msg);
            }
        };
        ev.onAddedToConversation = added =>
        {
            var intended = added.intendedRecipients.ToArray();
            if (intended.Any(a => a.userId == userId))
            {
                addedHandler(added);
            }
        };
        ev.onTyping = typing =>
        {
            var intended = typing.intendedRecipients.ToArray();
            if (intended.Any(a => a.userId == userId))
            {
                typingHandler(typing);
            }
        };

        var result = await ListenForMessages(ev);
        ev.onDisconnectRequest = () =>
        {
            result.Disconnect();
        };
        return ev;
    }

    private async Task BroadcastMessageSent(Message message)
    {
        var partners = (await GetChatParticipants(message.conversationId)).Where(c => c.userId != message.userId)
            .ToArray();
        if (partners.Length == 0)
            return;
        
        await Roblox.Cache.DistributedCache.redis.GetDatabase(0)
            .PublishAsync(redisMessagesChannel, JsonSerializer.Serialize(new MessageEvent()
            {
                message = message,
                intendedRecipients = partners,
            }));
    }

    public async Task<Message> SendMessage(long conversationId, long userId, string message)
    {
        // Message validation
        if (string.IsNullOrWhiteSpace(message))
            throw new RobloxException(400, 0, "Message cannot be empty.");
        if (message.Length > 255)
            throw new RobloxException(400, 0, "Message is too long.");

        // Permissions
        if (!await IsUserInConversation(conversationId, userId))
            throw new RobloxException(403, 3,
                "Failed to send GRPC request.\r\n\tService: roblox.chat.chatgateway.v1.ChatGatewayAPI\r\n\tMethod: SendMessageV3\r\n\tHost: 10.0.27.165:22105\r\n\tStatus: PermissionDenied (User is not part of the conversation. UserId: "+userId+". ConversationId: "+conversationId+". Context: None.)\r\n");
        
        // Insert
        var messageId = Guid.NewGuid().ToString();
        var created = DateTime.UtcNow;
        await db.ExecuteAsync("INSERT INTO user_conversation_message (id, conversation_id, user_id, message, created_at) VALUES (:id, :conversation_id, :user_id, :message, :created_at)",
            new
            {
                id = messageId,
                conversation_id = conversationId,
                user_id = userId,
                message = message,
                created_at = created,
            });
        // Mark as read for authenticated user
        await MarkMessageAsRead(conversationId, messageId, userId);
        // OK
        var msg = new Message()
        {
            conversationId = conversationId,
            createdAt = created,
            id = messageId,
            message = message,
            userId = userId,
        };
        // Broadcast
        await BroadcastMessageSent(msg);
        return msg;
    }

    public async Task StartTyping(long conversationId, long userId)
    {
        // Permissions
        if (!await IsUserInConversation(conversationId, userId))
            throw new RobloxException(403, 3,
                "Failed to send GRPC request.\r\n\tService: roblox.chat.chatgateway.v1.ChatGatewayAPI\r\n\tMethod: StartTyping\r\n\tHost: 10.0.27.165:22105\r\n\tStatus: PermissionDenied (User is not part of the conversation. UserId: "+userId+". ConversationId: "+conversationId+". Context: None.)\r\n");
        
        var ev = new TypingEvent()
        {
            conversationId = conversationId,
            endsAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (5 * 1000),
            isTyping = true,
            userId = userId,
        };
        ev.intendedRecipients = (await GetChatParticipants(conversationId)).Where(c => c.userId != userId);
        await Roblox.Cache.DistributedCache.redis.GetDatabase(0)
            .PublishAsync(redisTypingChannel, JsonSerializer.Serialize(ev));
    }

    public bool IsThreadSafe()
    {
        return true;
    }

    public bool IsReusable()
    {
        return true;
    }
}