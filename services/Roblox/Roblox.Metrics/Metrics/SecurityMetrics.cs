using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace Roblox.Metrics;

public static class SecurityMetrics
{
    public static void ReportBadCharacterFoundInAssetContentName(string contentName, string badCharacter, string method)
    {
        RobloxInfluxDb.WritePointInBackground(PointData.Measurement("WebsiteSecurity")
            .Field("name", "BadCharacterFoundInAssetContentName")
            .Field("badCharacter", badCharacter)
            .Field("contentName", contentName)
            .Field("method", method)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms)
        );
    }
    
    public static void ReportErrorDeletingAssetContent(string contentName, string stack, string message)
    {
        RobloxInfluxDb.WritePointInBackground(PointData.Measurement("WebsiteSecurity")
            .Field("contentName", contentName)
            .Field("name", "ErrorDeletingAssetContent")
            .Field("stack", stack)
            .Field("message", message)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms)
        );
    }
}
