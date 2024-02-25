using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Roblox.Models.Sessions;
using Roblox.Services;
using Roblox.Services.Exceptions;

namespace Roblox.Website.Hubs;

public class ChatHub : Hub
{
    private readonly IHubContext<ChatHub> _hubContext;
    
    public ChatHub(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }
    private static ConcurrentDictionary<string, Roblox.Dto.Chat.EventHandler> connectionIdToListener = new();

    private Roblox.Models.Sessions.UserSession? userSession
    {
        get
        {
            var dict = Context.GetHttpContext()?.Items;
            if (dict?.ContainsKey(Roblox.Website.Middleware.SessionMiddleware.CookieName) == true)
                return (UserSession?)dict[Middleware.SessionMiddleware.CookieName];

            return null;
        }
    }

    public async Task<string> ListenForMessages()
    {
        return Guid.NewGuid().ToString();
    }

    public override async Task OnConnectedAsync()
    {
        var ctx = Context.GetHttpContext();
        if (ctx == null)
            throw new RobloxException(400, 0, "BadRequest");
        if (userSession == null)
            throw new RobloxException(400, 0, "BadRequest");

        var id = Context.ConnectionId;
        var chat = Roblox.Services.ServiceProvider.GetOrCreate<ChatService>();
        if (connectionIdToListener.ContainsKey(id))
            throw new Exception("EventHandler already set");

        connectionIdToListener[id] = await chat.ListenForMessages(userSession.userId, ev =>
        {
            Task.Run(async () =>
            {
                try
                {
                    var conn = _hubContext.Clients.Client(id);
                    await conn.SendAsync("ChatMessageReceived", JsonSerializer.Serialize(ev.message));
                }
                catch (Exception e)
                {
                    Console.WriteLine("[error] error sending message for chat hub: {0}", e.Message);
                }
            });
        }, added =>
        {
            Task.Run(async () =>
            {
                try
                {
                    var conn = _hubContext.Clients.Client(id);
                    await conn.SendAsync("ChatConversationAdded", JsonSerializer.Serialize(added));
                }
                catch (Exception e)
                {
                    Console.WriteLine("[error] error sending message for ChatConversationAdded hub: {0}", e.Message);
                }
            });
        }, t =>
        {
            Task.Run(async () =>
            {
                try
                {
                    var conn = _hubContext.Clients.Client(id);
                    await conn.SendAsync("ChatTyping", JsonSerializer.Serialize(t));
                }
                catch (Exception e)
                {
                    Console.WriteLine("[error] error sending message for ChatTyping hub: {0}", e.Message);
                }
            });
        });
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine("Destroying connection for {0}", Context.ConnectionId);
        if (connectionIdToListener.ContainsKey(Context.ConnectionId))
        {
            connectionIdToListener[Context.ConnectionId].Disconnect();
            connectionIdToListener.Remove(Context.ConnectionId, out _);
        }

        return base.OnDisconnectedAsync(exception);
    }
}