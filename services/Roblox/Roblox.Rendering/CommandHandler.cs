using System.Buffers;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Web;
using Roblox.Logging;

namespace Roblox.Rendering
{
    public static class CommandHandler
    {
        private static System.Threading.Mutex mux { get; set; } = new();
        private static ClientWebSocket? ws { get; set; }
        private static Dictionary<string, Func<RenderResponse<Stream>,int>> resultListeners { get; } = new();
        private static Uri wsUrl { get; set; }

        public static void Configure(string baseUrl, string authorization)
        {
            var url = new Uri(baseUrl + "?key=" + HttpUtility.UrlEncode(authorization));
            wsUrl = url;

            Task.Run(async () =>
            {
                await ConnectionManager();
            });
        }

        private static async Task ListenForMessages()
        {
            // allocate 8mb
            using var memory = MemoryPool<byte>.Shared.Rent(1024 * 1024 * 8);

            while (true)
            {
                try
                {
                    var result = await ws.ReceiveAsync(memory.Memory, CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("[info] Render websocket closed. Re-opening...");
                        continue;
                    }

                    var msg = Encoding.UTF8.GetString(memory.Memory.Span.Slice(0, result.Count));

                    Console.WriteLine("got ws message. result={0}", msg.Substring(0,75)+"...");
                    var decoded = JsonSerializer.Deserialize<RenderResponse<string>>(msg);
                    if (decoded == null)
                    {
                        Console.WriteLine("Got invalid WS message - it was null");
                        continue;
                    }
                    var newResponse = new RenderResponse<Stream>()
                    {
                        id = decoded.id,
                        status = decoded.status,
                        data = null,
                    };

                    // data is null when statusCode != 200
                    if (decoded.data != null)
                    {
                        var bytes = Convert.FromBase64String(decoded.data);
                        newResponse.data = new MemoryStream(bytes);
                    }

                    mux.WaitOne();
                    try
                    {
                        var hasListener = resultListeners.ContainsKey(decoded.id);
                        if (hasListener)
                        {
                            resultListeners[decoded.id](newResponse);
                            resultListeners.Remove(decoded.id);
                        }
                        else
                        {
                            Console.WriteLine("[warning] got message for item without listener. id = {0}", decoded.id);
                        }
                    }
                    finally
                    {
                        mux.ReleaseMutex();
                    }
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Got error in ws connection {0}", e.Message);
                    throw;
                }
            }
        }
        
        private static async Task ConnectionManager()
        {
            while (true)
            {
                try
                {
                    mux.WaitOne();
                    ws ??= new ClientWebSocket();
                    var wsCurrentState = ws.State;
                    mux.ReleaseMutex();
                    
                    if (wsCurrentState is WebSocketState.Aborted or WebSocketState.Closed or WebSocketState.None or WebSocketState.CloseReceived or WebSocketState.CloseSent)
                    {
                        Console.WriteLine("[info] ws connection is in state {0}, so we are re-connecting", ws.State);
                        mux.WaitOne();
                        ws = new ClientWebSocket();
                        mux.ReleaseMutex();
                        await ws.ConnectAsync(wsUrl, CancellationToken.None);
                    }
                    await ListenForMessages();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[info] ConnectionManager got error in ws connection {0}", e.Message);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }

        private static async Task<RenderResponse<Stream>> SendCommand(string command, IEnumerable<dynamic> arguments, CancellationToken? cancellationToken)
        {
            var id = Guid.NewGuid().ToString();
            var cmd = new RenderRequest()
            {
                command = command,
                args = arguments,
                id = id,
            };
            var res = new TaskCompletionSource<RenderResponse<Stream>>();
            var responseMutex = new Mutex();
            
            // Setup listener
            mux.WaitOne();
            resultListeners[id] = stream =>
            {
                Console.WriteLine("[info] SendCommand() over");
                lock (responseMutex)
                {
                    res.SetResult(stream);
                }

                return 0; 
            };
            mux.ReleaseMutex();
            // Send message
            var bits = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cmd));
            while (ws is not {State: WebSocketState.Open})
            {
                //Writer.Info(LogGroup.GeneralRender, "Ws not available, retry in a second");
#if DEBUG 
                await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken  ?? CancellationToken.None);
#else
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken  ?? CancellationToken.None);
#endif
                if (cancellationToken is {IsCancellationRequested: true})
                    throw new TaskCanceledException();
            }
            await ws.SendAsync(bits, WebSocketMessageType.Text, true, cancellationToken ?? CancellationToken.None);

            await using var register = cancellationToken?.Register(() =>
            {
                mux.WaitOne();
                resultListeners.Remove(id);
                mux.ReleaseMutex();
                // TODO: Would be nice if we could send a message to WS telling it to cancel the task
                // TrySetCanceled instead of SetCanceled due to WEB-35
                lock (responseMutex)
                {
                    if (res.TrySetCanceled(cancellationToken.Value) && command != "Cancel")
                    {
                        SendCommand("Cancel", new List<dynamic>()
                        {
                            id,
                        }, CancellationToken.None);
                    }
                }
            });
            var resp = await res.Task;
            return resp;
        }
        
        
        private static async Task<Stream> SendCmdWithErrHandlingAsync(string cmd, IEnumerable<dynamic> arguments, CancellationToken? cancellationToken = null)
        {
            var result = await SendCommand(cmd, arguments, cancellationToken);
            if (result.status != 200) throw new Exception("Render failed with status = " + result.status);
            if (result.data == null) throw new Exception("Null stream returned from SendCommand");
            return result.data;
        }

        public static async Task<Stream> RequestPlayerThumbnail(AvatarData data, CancellationToken? cancellationToken = null)
        {
            if (data.playerAvatarType != "R6")
                throw new Exception("Invalid PlayerAvatarType");
            
            // todo: do we need to get assetTypeId here, or can we just expect caller to get it for us?
            var w = new Stopwatch();
            w.Start();
            
            var result = await SendCommand("GenerateThumbnail",
                new List<dynamic> {data}, cancellationToken);
            w.Stop();
            if (result.status != 200 || result.data == null)
            {
                if (result.data == null && result.status == 200)
                    Roblox.Metrics.RenderMetrics.ReportRenderAvatarThumbnailFailureDueToNullBody(data.userId);
                Roblox.Metrics.RenderMetrics.ReportRenderAvatarThumbnailFailure(data.userId);
                throw new Exception("Render failed with status = " + result.status);
            }
            Metrics.RenderMetrics.ReportRenderAvatarThumbnailTime(data.userId, w.ElapsedMilliseconds);
            return result.data;
        }

        public static async Task<Stream> RequestPlayerHeadshot(AvatarData data, CancellationToken? cancellationToken = null)
        {
            if (data.playerAvatarType != "R6")
                throw new Exception("Invalid PlayerAvatarType");
            
            // todo: do we need to get assetTypeId here, or can we just expect caller to get it for us?
            return await SendCmdWithErrHandlingAsync("GenerateThumbnailHeadshot", new List<dynamic> {data}, cancellationToken);
        }

        public static async Task<Stream> RequestTextureThumbnail(long assetId, int assetTypeId, CancellationToken? cancellationToken = null)
        {
            return await SendCmdWithErrHandlingAsync("GenerateThumbnailTexture", new List<dynamic>
            {
                assetId, 
                assetTypeId
            }, cancellationToken);
        }
        
        public static async Task<Stream> RequestAssetThumbnail(long assetId, CancellationToken? cancellationToken = null)
        {
            return await SendCmdWithErrHandlingAsync("GenerateThumbnailAsset", new List<dynamic>
            {
                assetId, 
            }, cancellationToken);
        }
        

        public static async Task<Stream> RequestHeadThumbnail(long assetId, CancellationToken? cancellationToken = null)
        {
            return await SendCmdWithErrHandlingAsync("GenerateThumbnailHead", new List<dynamic>
            {
                assetId, 
            }, cancellationToken);
        }
        public static async Task<Stream> RequestAssetMesh(long assetId, CancellationToken? cancellationToken = null)
        {
            return await SendCmdWithErrHandlingAsync("GenerateThumbnailMesh", new List<dynamic>
            {
                assetId, 
            }, cancellationToken);
        }

        public static async Task<Stream> RequestPlaceConversion(string base64EncodedPlace, CancellationToken? cancellationToken = null)
        {
            return await SendCmdWithErrHandlingAsync("ConvertRobloxPlace", new List<dynamic>
            {
                base64EncodedPlace, 
            }, cancellationToken);
        }

        public static async Task<Stream> RequestHatConversion(string base64EncodedHat,
            CancellationToken? cancellationToken = null)
        {
            return await SendCmdWithErrHandlingAsync("ConvertHat", new List<dynamic>()
            {
                base64EncodedHat,
            });
        }
        
        public static async Task<Stream> RequestAssetGame(long assetId, int x, int y, CancellationToken? cancellationToken = null)
        {
            return await SendCmdWithErrHandlingAsync("GenerateThumbnailGame", new List<dynamic>
            {
                assetId,
                x,
                y,
            }, cancellationToken);
        }

        public static async Task<Stream> RequestAssetTeeShirt(long assetId, long contentId, CancellationToken? cancellationToken = null)
        {
            return await SendCmdWithErrHandlingAsync("GenerateThumbnailTeeShirt", new List<dynamic>
            {
                assetId,
                contentId,
            }, cancellationToken);
        }
    }
}

