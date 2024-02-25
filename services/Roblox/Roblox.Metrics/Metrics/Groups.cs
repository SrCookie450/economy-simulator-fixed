using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Roblox.Logging;

namespace Roblox.Metrics;

public static class GroupMetrics
{
    private static void LogFloodCheck(string group, string msg, params object?[] obj)
    {
        Writer.Info(LogGroup.FloodCheck, "[" + group + "] " + msg, obj);
    }

    public static void ReportWallPostFloodCheck(long groupId, long userId, long postCount)
    {
        LogFloodCheck("GroupWallPostFloodCheck", "user {0} hit limit for group {1} at {2} posts", userId, groupId,
            postCount);
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("GroupWallPostFloodCheck")
            .Field("userId", userId)
            .Field("groupId", groupId)
            .Field("postCount", postCount)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }

    public static void ReportGlobalWallPostFloodCheck(long groupId, long userId, long postCount)
    {
        LogFloodCheck("GlobalGroupWallPostFloodCheck", "user {0} hit limit for group {1} at {2} posts", userId, groupId,
            postCount);
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("GlobalGroupWallPostFloodCheck")
            .Field("userId", userId)
            .Field("groupId", groupId)
            .Field("postCount", postCount)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }

    public static void ReportGlobalWallPostFloodCheckForSpecificGroup(long groupId, long postCount)
    {
        LogFloodCheck("GlobalGroupWallPostFloodCheckForGroup", "limit hit for group {0} at {1} posts", groupId,
            postCount);
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("GlobalGroupWallPostFloodCheckForGroup")
            .Field("groupId", groupId)
            .Field("postCount", postCount)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
}