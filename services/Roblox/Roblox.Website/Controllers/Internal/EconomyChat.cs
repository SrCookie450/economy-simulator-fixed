using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Roblox.EconomyChat;
using Roblox.EconomyChat.Models;
using Roblox.Services.Exceptions;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/api/economy-chat/v1")]
public class EconomyChat : ControllerBase
{
    private ChatService chatService { get; }

    public EconomyChat()
    {
        chatService = new ChatService();
    }

    [HttpGet("metadata")]
    public Roblox.EconomyChat.Models.Metadata GetMetadata()
    {
        return new()
        {
            isEnabled = true,
        };
    }

    [HttpGet("channels/list")]
    public IEnumerable<Channel> GetChannels()
    {
        return Channel.channels;
    }

    [HttpGet("channels/{channelId:long}/messages")]
    public async Task<IEnumerable<ChannelChatMessage>> GetMessages(long channelId, long startMessageId, int limit)
    {
        if (limit is > 100 or < 1)
            limit = 100;
        return await chatService.GetMessagesInChannel(channelId, startMessageId, limit);
    }

    [HttpPost("channels/{channelId:long}/typing")]
    public async Task MarkAsTyping(long channelId)
    {
        if (userSession is null)
            throw new RobloxException(403, 0, "Forbidden");
        await chatService.ToggleTyping(userSession.userId, channelId, true);
    }
    
    [HttpDelete("channels/{channelId:long}/typing")]
    public async Task MarkAsNotTyping(long channelId)
    {
        if (userSession is null)
            throw new RobloxException(403, 0, "Forbidden");
        await chatService.ToggleTyping(userSession.userId, channelId, false);
    }

    [HttpGet("channels/{channelId:long}/read")]
    public async Task<UnreadMessageCount> GetUnreadMessageCount(long channelId)
    {
        if (userSession is null)
            throw new RobloxException(403, 0, "Forbidden");
        return await chatService.GetUnreadMessageCount(userSession.userId, channelId);
    }

    [HttpPost("channels/{channelId:long}/read")]
    public async Task MarkChannelAsRead(long channelId)
    {
        await chatService.SetReadMessage(userSession.userId, channelId);
    }

    [HttpPost("channels/{channelId:long}/send")]
    public async Task<ChatMessage> CreateMessage(long channelId, [Required, FromBody] CreateMessageRequest request)
    {
        request.channelId = channelId;
        return await chatService.CreateChannelMessage(userSession.userId, request);
    }

    [HttpDelete("channels/{channelId:long}/messages/{messageId:long}")]
    public async Task DeleteMessage(long channelId, long messageId)
    {
        if (userSession is null)
            throw new RobloxException(403, 0, "Forbidden");
        await chatService.RemoveMessage(userSession.userId, messageId);
    }
}