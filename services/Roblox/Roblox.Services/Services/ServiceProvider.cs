using System.Diagnostics;

namespace Roblox.Services;

public static class ServiceProvider
{
    private static Dictionary<string, IService> cachedServices { get; set; } = new();
    private static Mutex servicesMux { get; set; } = new();
    
    public static T GetOrCreate<T>(ServiceBase? parent = null) where T : ServiceBase, IDisposable, IService, new()
    {
        var serviceName = typeof(T).FullName ?? typeof(T).Name;
        if (parent == null)
        {
            servicesMux.WaitOne();
            if (cachedServices.TryGetValue(serviceName, out var cService))
            {
                servicesMux.ReleaseMutex();
                return (T)cService;
            }
        }
        var service = new T();
        if (parent != null)
        {
            service.transactionConnection = parent.transactionConnection;
        }
        else if (service.IsReusable() && service.IsThreadSafe())
        {
            cachedServices.Add(serviceName, service);
        }

        if (parent == null)
        {
            servicesMux.ReleaseMutex();
        }
        return service;
    }
}