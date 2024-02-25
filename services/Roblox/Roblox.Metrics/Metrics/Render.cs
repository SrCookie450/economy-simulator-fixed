using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Roblox.Logging;

namespace Roblox.Metrics;

public static class RenderMetrics
{
    private static void LogMetric(string group, string msg, params object?[] obj)
    {
        Writer.Info(LogGroup.FloodCheck, "[" + group + "] " + msg, obj);
    }

    public static void ReportRenderAvatarThumbnailFailure(long userId)
    {
        LogMetric("RenderAvatarThumbnailFailure", "avatar render failed");
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("RenderAvatarThumbnailFailure")
            .Field("userId", userId)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
    
    
    public static void ReportRenderAvatarThumbnailFailureDueToNullBody(long userId)
    {
        LogMetric("RenderAvatarThumbnailFailureDueToNullBody", "avatar render failed");
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("RenderAvatarThumbnailFailureDueToNullBody")
            .Field("userId", userId)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }

    public static void ReportRenderAvatarThumbnailTime(long userId, long timeInMilliseconds)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("RenderAvatarThumbnail")
            .Field("userId", userId)
            .Field("timeInMilliseconds", timeInMilliseconds)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
}