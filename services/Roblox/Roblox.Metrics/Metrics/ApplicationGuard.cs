using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Roblox.Logging;

namespace Roblox.Metrics;

public static class ApplicationGuardMetrics
{
    public static void ReportBlockedUserAgent(string userAgent)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("UserAgentBlocked")
            .Field("userAgent", userAgent)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms)
        );
    }

    public static void ReportAllowedUserAgent(string userAgent)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("UserAgentAllowed")
            .Field("userAgent", userAgent)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms)
        );
    }
    
    public static void ReportCaptchaSuccessForUserAgent(string userAgent)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("UserAgentCaptchaCompleted")
            .Field("userAgent", userAgent)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms)
        );
    }

    public static void ReportOneRequest(string pathWithoutQuery, long durationInMilliseconds)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("WebRequest")
            .Tag("path", pathWithoutQuery)
            .Field("durationInMilliseconds", durationInMilliseconds)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms)
        );
    }
}