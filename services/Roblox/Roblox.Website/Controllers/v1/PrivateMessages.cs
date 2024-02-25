using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Roblox.Exceptions;
using Roblox.Services.App.FeatureFlags;
using Roblox.Website.WebsiteModels.Users;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/privatemessages/v1")]
public class PrivateMessagesControllerV1 : ControllerBase
{
    private void FeatureCheck()
    {
        FeatureFlags.FeatureCheck(FeatureFlag.PrivateMessagesEnabled);
    }
    
    public static List<dynamic> GlobalMessages = new List<dynamic>()
    {
        new
        {
            id = 1,
            sender = new
            {
                id = 1,
                name = "ROBLOX",
                displayName = "ROBLOX",
            },
            subject = "Welcome to our site!",
            body = "Amogus drip official no virus 2019 hindi subtitles",
            created = "2021-01-13T12:00:00.42Z",
            updated = "2021-01-13T12:00:00.42Z",
        }
    };
    [HttpGet("announcements/metadata")]
    public dynamic GetAnnouncementsMetadata()
    {
        return new
        {
            numOfAnnouncements = GlobalMessages.Count,
        };
    }

    [HttpGet("announcements")]
    public dynamic GetAnnouncements()
    {
        return new
        {
            collection = GlobalMessages,
            totalCollectionSize = GlobalMessages.Count,
        };
    }

    [HttpGet("messages/unread/count")]
    public async Task<dynamic> GetUnreadMessagesCount()
    {
        FeatureCheck();
        var res = await services.privateMessages.CountUnreadMessages(userSession.userId);
        return new
        {
            count = res,
        };
    }

    [HttpPost("messages/send")]
    public async Task<dynamic> SendMessage([Required,FromBody] SendMessageRequest request)
    {
        FeatureCheck();
        if (await services.privateMessages.IsFloodChecked(userSession.userId))
            return new
            {
                success = false,
                shortMessage = "FloodCheck",
                message = "Too many messages. Try again in a few minutes.",
            };
        
        try
        {
            await services.privateMessages.CreateMessage(request.recipientid, userSession.userId, request.subject,
                request.body, request.replyMessageId, request.includePreviousMessage);
        }
        catch (ArgumentException e)
        {
            return new
            {
                success = false,
                shortMessage = "GeneralError",
                message = e.Message,
            };
        }
        return new
        {
            success = true,
            shortMessage = "Success",
            message = "Successfully sent message.",
        };
    }

    [HttpPost("messages/mark-read")]
    public async Task MarkMessagesAsRead([Required, FromBody] MultiUpdateMessagesRequest request)
    {
        FeatureCheck();
        foreach (var item in request.messageIds)
        {
            var info = await services.privateMessages.GetMessageById(item);
            if (info.receiverId != userSession.userId)
                throw new ArgumentException("Invalid messageId");
            await services.privateMessages.SetReadStatus(item, true);
        }
    }
    
    [HttpPost("messages/mark-unread")]
    public async Task MarkMessagesAsUnread([Required, FromBody] MultiUpdateMessagesRequest request)
    {
        FeatureCheck();
        foreach (var item in request.messageIds)
        {
            var info = await services.privateMessages.GetMessageById(item);
            if (info.receiverId != userSession.userId)
                throw new ArgumentException("Invalid messageId");
            await services.privateMessages.SetReadStatus(item, false);
        }
    }
    
    [HttpPost("messages/archive")]
    public async Task MarkMessagesAsArchived([Required, FromBody] MultiUpdateMessagesRequest request)
    {
        FeatureCheck();
        foreach (var item in request.messageIds)
        {
            var info = await services.privateMessages.GetMessageById(item);
            if (info.receiverId != userSession.userId)
                throw new ArgumentException("Invalid messageId");
            await services.privateMessages.SetArchiveStatus(item, true);
        }
    }
    
    [HttpPost("messages/unarchive")]
    public async Task MarkMessagesAsUnarchived([Required, FromBody] MultiUpdateMessagesRequest request)
    {
        FeatureCheck();
        foreach (var item in request.messageIds)
        {
            var info = await services.privateMessages.GetMessageById(item);
            if (info.receiverId != userSession.userId)
                throw new ArgumentException("Invalid messageId");
            await services.privateMessages.SetArchiveStatus(item, false);
        }
    }

    [HttpGet("messages")]
    public async Task<dynamic> GetMessages(string messageTab, int pageSize, int pageNumber)
    {
        FeatureCheck();
        if (pageSize is > 100 or < 0) pageSize = 10;
        return await services.privateMessages.GetMessages(userSession.userId, messageTab, pageSize,
            pageSize * pageNumber);
    }

    [HttpGet("messages/{messageId:long}")]
    public async Task<dynamic> GetMessageById(long messageId)
    {
        FeatureCheck();
        var msg = await services.privateMessages.GetMessageById(messageId);
        if (msg.receiverId != userSession.userId && msg.senderId != userSession.userId)
        {
            throw new BadRequestException();
        }

        return new
        {
            id = messageId,
            sender = new
            {
                id = msg.senderId,
                name = msg.senderUsername,
                displayName = msg.senderUsername,
            },
            recipient = new
            {
                id = msg.receiverId,
                name = msg.receiverUsername,
                displayName = msg.receiverUsername,
            },
            subject = msg.subject,
            body = msg.body,
            created = msg.created,
            updated = msg.updated,
            isRead = msg.isRead,
            isSystemMessage = msg.isSystemMessage,
            isReportAbuseDisplayed = msg.isReportAbuseDisplayed,
        };
    }
    
}