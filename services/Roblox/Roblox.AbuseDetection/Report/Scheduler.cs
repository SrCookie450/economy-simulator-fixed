using Roblox.Logging;
using Roblox.Services;

namespace Roblox.AbuseDetection;

public static class Scheduler
{
    private static async Task RunTaskIfAvailable(string taskName, Func<Task> cb, TimeSpan executionFrequency, bool exclusiveLockRequired)
    {
        // Check if running (quick)
        
    }
    
    public static void AddTask(string taskName, Func<Task> cb, TimeSpan executionFrequency, bool exclusiveLockRequired)
    {
        Task.Run(async () =>
        {
            try
            {
                await RunTaskIfAvailable(taskName, cb, executionFrequency, exclusiveLockRequired);
            }
            catch (Exception e)
            {
                Writer.Info(LogGroup.AbuseDetection, "Fatal error in RunTaskIfAvailable.{0}: {1}\n{2}",taskName, e.Message, e.StackTrace);
            }
        });
    }
}