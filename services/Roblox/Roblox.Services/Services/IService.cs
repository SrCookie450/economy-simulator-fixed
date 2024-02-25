namespace Roblox.Services;

public interface IService
{ 
    /// <summary>
    /// Whether service methods can be called by multiple threads at once
    /// </summary>
    /// <returns>Whether the service is thread safe or not</returns>
    bool IsThreadSafe();
    /// <summary>
    /// Whether the service can be re-used for multiple requests. Ignored if <see cref="IsThreadSafe"/> is false.
    /// </summary>
    /// <returns>Whether the service can be re-used or not</returns>
    bool IsReusable();
}

