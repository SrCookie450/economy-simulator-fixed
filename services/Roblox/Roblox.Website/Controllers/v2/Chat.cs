using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Roblox.Models.Chat;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/chat/v2")]
public class ChatControllerV2 : ControllerBase
{
    [HttpGet("metadata")]
    public dynamic GetMetadata()
    {
        return new
        {
            isChatEnabledByPrivacySetting = 0, languageForPrivacySettingUnavailable = "Chat is currently unavailable",
            maxConversationTitleLength = 150, numberOfMembersForPartyChrome = 6,
            partyChromeDisplayTimeStampInterval = 300000, signalRDisconnectionResponseInMilliseconds = 3000,
            typingInChatFromSenderThrottleMs = 5000, typingInChatForReceiverExpirationMs = 8000,
            relativeValueToRecordUiPerformance = 0.0, isChatDataFromLocalStorageEnabled = false,
            chatDataFromLocalStorageExpirationSeconds = 30, isUsingCacheToLoadFriendsInfoEnabled = false,
            cachedDataFromLocalStorageExpirationMS = 30000,
            senderTypesForUnknownMessageTypeError = new List<string>() {"User"},
            isInvalidMessageTypeFallbackEnabled = false, isRespectingMessageTypeEnabled = true,
            validMessageTypesWhiteList = new List<string>() {"PlainText", "Link"},
            shouldRespectConversationHasUnreadMessageToMarkAsRead = true, isVoiceChatForClientSideEnabled = false,
            isAliasChatForClientSideEnabled = true, isPlayTogetherForGameCardsEnabled = true,
            isRoactChatEnabled = false
        };
    }

    [HttpGet("get-unread-conversation-count")]
    public dynamic GetUnreadConversationCount()
    {
        // todo
        throw new NotImplementedException();
        return new
        {
            count = 0,
        };
    }

    [HttpGet("chat-settings")]
    public async Task<dynamic> GetChatSettings()
    {
        return new
        {
            chatEnabled = FeatureFlags.IsEnabled(FeatureFlag.WebsiteChat),
            isActiveChatUser = true, // todo
        };
    }

    [HttpGet("get-user-conversations")]
    public async Task<dynamic> GetAuthenticatedUserConversations(int pageNumber, int pageSize)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.WebsiteChat);
        if (pageSize > 100 || pageSize < 0)
            throw new RobloxException(400, 0, "BadRequest");
        var offset = pageNumber * pageSize - pageSize;
        var conversations = await services.chat.GetUserConversations(safeUserSession.userId);
        var response = new List<dynamic>();
        foreach (var item in conversations.Skip(offset))
        {
            if (response.Count >= pageSize)
                break;
            var participants = (await services.chat.GetChatParticipants(item.id)).ToArray();
            var names = (await services.users.MultiGetUsersById(participants.Select(c => c.userId))).ToArray();
            var hasUnRead = await services.chat.DoesHaveUnreadMessages(item.id, safeUserSession.userId);
            if (item.conversationType == ConversationType.OneToOneConversation)
            {
                var areFriends = await services.friends.AreAlreadyFriends(participants[0].userId, participants[1].userId);
                if (!areFriends)
                    continue;
            }
            response.Add(new
            {
                id = item.id,
                title = item.title,
                hasUnreadMessages = hasUnRead,
                participants = participants.Select(c => new
                {
                    type = "User",
                    targetId = c.userId,
                    name = names.FirstOrDefault(a => a.id == c.userId)?.name,
                }),
                conversationType = item.conversationType,
                conversationTitle = new
                {
                    titleForViewer = item.title,
                    isDefaultTitle = item.title == null,
                },
                conversationUniverse = (object?)null, // todo?
            });
        }

        return response;
    }

    [HttpPost("mark-as-read")]
    public async Task MarkMessageAsRead([Required, FromBody] Dto.Chat.MarkAsReadRequest request)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.WebsiteChat);
        await services.chat.MarkMessageAsRead(request.conversationId, request.endMessageId, safeUserSession.userId);
    }

    [HttpPost("start-one-to-one-conversation")]
    public async Task<dynamic> StartOneToOneConversation([Required, FromBody] Dto.Chat.StartConversationRequest request)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.WebsiteChat);
        var result = await services.chat.CreateOneToOneConversation(safeUserSession.userId, request.participantUserId);
        return new
        {
            conversation = new
            {
                id = result.id,
            },
        };
    }

    [HttpPost("update-user-typing-status")]
    public async Task UpdateUserTypingStatus([Required, FromBody] Dto.Chat.UpdateTypingStatusRequest request)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.WebsiteChat);
        await services.chat.StartTyping(request.conversationId, safeUserSession.userId);
    }
    
    [HttpPost("send-message")]
    public async Task<dynamic> SendMessage([Required, FromBody] Dto.Chat.SendMessageRequest request)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.WebsiteChat);
        var resp = await services.chat.SendMessage(request.conversationId, safeUserSession.userId, request.message);
        return new
        {
            content = resp.message,
            messageId = resp.id,
            sent = resp.createdAt,
            messageType = "PlainText",
        };
    }

    [HttpGet("multi-get-latest-messages")]
    public async Task<dynamic> MultiGetLatestMessages(string conversationIds)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.WebsiteChat);
        var ids = conversationIds.Split(",").Select(long.Parse).Distinct().ToArray();
        if (ids.Length == 0 || ids.Length > 100)
            throw new RobloxException(400, 0, "BadRequest");
        
        var result = new List<dynamic>();
        foreach (var id in ids)
        {
            if (!await services.chat.IsUserInConversation(id, safeUserSession.userId))
                throw new RobloxException(403, 0, "Forbidden");
            var latest = await services.chat.GetLatestMessageInConversation(id);
            var isRead = latest == null || await services.chat.IsRead(latest.id, id, safeUserSession.userId);

            result.Add(new
            {
                conversationId = id,
                chatMessages = latest!=null ? new []
                {
                    new
                    {
                        id = latest.id,
                        sent = latest.createdAt,
                        read = isRead,
                        senderTargetId = latest.userId,
                        content = latest.message,
                    }
                } : ArraySegment<dynamic>.Empty,
            });
        }

        return result;
    }

    [HttpGet("get-messages")]
    public async Task<dynamic> GetMessages(long conversationId, int pageSize, string? exclusiveStartMessageId = null)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.WebsiteChat);
        exclusiveStartMessageId ??= "";
        if (pageSize > 100 || pageSize < 0)
            throw new RobloxException(400, 0, "BadRequest");
        if (!await services.chat.IsUserInConversation(conversationId, safeUserSession.userId))
            throw new RobloxException(403, 0, "Forbidden");
        
        var messages = await services.chat.GetLatestMessagesInConversation(conversationId, exclusiveStartMessageId, pageSize);
        
        var response = new List<dynamic>();
        foreach (var message in messages)
        {
            response.Add(new
            {
                id = message.id,
                sent = message.createdAt,
                read = await services.chat.IsRead(message.id, message.conversationId, safeUserSession.userId),
                messageType = "PlainText",
                senderTargetId = message.userId,
                content = message.message,
            });
        }

        return response;
    }
}