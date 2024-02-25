namespace Roblox.Logging;

public enum LogGroup
{
    HttpRequest = 1,
    GeneralRender,
    AvatarRenderThumb,
    AvatarRenderHead,
    AssetRender,
    GameIconRender,
    GameThumbnailRender,
    AssetValidation,
    ItemPurchase,
    ItemPurchaseResale,
    TradeSend,
    TradeAccept,
    Lottery,
    Group,
    GroupRolesetUpdate,
    FeatureFlags,
    AbuseDetection,
    Captcha,
    FloodCheck,
    FixBrokenThumbnails,
    SignUp,
    Metrics,
    AssetDelivery,
    AudioConversion,
    AsyncLimit,
    GameServerJoin,
    RealRobloxApi,
    CurrencyExchange,
    DistributedLock,
    PlaceConversion,
    PerformanceDebugging,
    ApplicationSocial,
    UserSessions,
    RealTimeChat,
    FixAssetImageMetadata,
    AdminApi,
}

public class Writer
{
#if DEBUG 
    public static int logLevel = 1;
#else
    public static int logLevel = 10;
#endif

    public static Dictionary<LogGroup, int> logLevels = new()
    {
        {LogGroup.HttpRequest, 10},
        {LogGroup.GeneralRender, 10},
        {LogGroup.AvatarRenderThumb, 4},
        {LogGroup.AvatarRenderHead, 4},
        {LogGroup.AssetRender, 4},
        {LogGroup.GameIconRender, 4},
        {LogGroup.GameThumbnailRender, 4},
        {LogGroup.AssetValidation, 4},
        {LogGroup.ItemPurchase, 100},
        {LogGroup.ItemPurchaseResale, 100},
        {LogGroup.TradeSend, 100},
        {LogGroup.TradeAccept, 100},
        {LogGroup.Lottery, 100},
        {LogGroup.Group, 10},
        {LogGroup.GroupRolesetUpdate, 1},
        {LogGroup.FeatureFlags, 100},
        {LogGroup.AbuseDetection, 100},
        {LogGroup.Captcha, 100},
        {LogGroup.FloodCheck, 100},
        {LogGroup.FixBrokenThumbnails, 100},
        {LogGroup.SignUp, 100},
        {LogGroup.Metrics, 100},
        {LogGroup.AssetDelivery, 100},
        {LogGroup.AudioConversion, 100},
        {LogGroup.AsyncLimit, 0},
        {LogGroup.GameServerJoin, 100},
        {LogGroup.CurrencyExchange, 100},
        {LogGroup.RealRobloxApi, 100},
        {LogGroup.DistributedLock, 100},
        {LogGroup.PlaceConversion, 100},
        {LogGroup.PerformanceDebugging, 5},
        {LogGroup.ApplicationSocial, 5},
        {LogGroup.UserSessions, 1},
        {LogGroup.RealTimeChat, 10},
        {LogGroup.FixAssetImageMetadata, 100},
        {LogGroup.AdminApi, 100},
    };

    private static List<Func<string,int>> onLogCallbacks { get; set; } = new();
    private static Mutex onLogCbMux { get; } = new();

    public static void OnLog(Func<string,int> cb)
    {
        lock (onLogCbMux)
        {
            onLogCallbacks.Add(cb);
        }
    }

    public static void Info(LogGroup group, string message, params object?[] fmt)
    {
        if (!logLevels.ContainsKey(group))
            return;
        
        if (logLevels[group] >= logLevel)
        {
            var msg = string.Format("[" + group.ToString() + "] " + DateTime.UtcNow.ToString("O") + " " + message, fmt);
            Console.WriteLine(msg);
            lock (onLogCbMux)
            {
                onLogCallbacks.ForEach(v => v(msg));
            }
        }
    }

    public class LogWithId : IDisposable
    {
        private string id { get; set; }
        private LogGroup group { get; set; }
        private List<string>? loggedStrings { get; set; }
        public LogWithId(LogGroup logGroup)
        {
            group = logGroup;
            id = Guid.NewGuid().ToString();
        }

        public string GetId()
        {
            return id;
        }
        
        public void Log(string message, params object?[] fmt)
        {
            var str = string.Format(
                "[" + group.ToString() + "] " + DateTime.UtcNow.ToString("O") + " " + id + " " + message, fmt);
            loggedStrings ??= new List<string>();
            loggedStrings.Add(str);
            if (logLevels[group] >= logLevel)
            {
                Console.WriteLine(str);
            }
        }
        
        public void Info(string message, params object?[] fmt)
        {
            Log(message, fmt);
        }

        public List<string> GetLogList()
        {
            return loggedStrings ?? new List<string>();
        }
        
        public string GetLoggedStrings()
        {
            return string.Join("\n", GetLogList());
        }

        public void Dispose()
        {
            loggedStrings?.Clear();
            loggedStrings = null;
        }
    }
    
    public static LogWithId CreateWithId(LogGroup group)
    {
        return new LogWithId(group);
    }
}