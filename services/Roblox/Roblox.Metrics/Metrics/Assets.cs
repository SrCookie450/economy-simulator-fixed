using System.Diagnostics;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Roblox.Logging;

namespace Roblox.Metrics;

public static class AssetMetrics
{
    public static void ReportInvalidClothingImageFormatUploadAttempt(string formatName)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("InvalidShirtImageFormatUploadAttempt")
            .Field("format", formatName)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms)
        );
    }

    public static void ReportInvalidClothingFileUploadAttempt(string error)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("InvalidClothingFileUploadAttempt")
            .Field("error", error)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms)
        );
    }
}