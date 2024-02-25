using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Roblox.Logging;

namespace Roblox.Metrics;

public static class UserMetrics
{
    private static void LogFloodCheck(string group, string msg, params object?[] obj)
    {
        Writer.Info(LogGroup.FloodCheck, "[" + group + "] " + msg, obj);
    }

    public static void ReportGlobalUploadsFloodcheckReached(long userId)
    {
        LogFloodCheck("GlobalUploadsFloodCheck", "user {0} hit global pending uploads flood check", userId);
        var point = PointData
            .Measurement("GlobalUploadsFloodCheck")
            .Field("userId", userId.ToString())
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);
        RobloxInfluxDb.WritePointInBackground(point);
    }

    public static void ReportPendingAssetsFloodCheckReached(long userId)
    {
        LogFloodCheck("PendingAssetsFloodCheck", "user {0} hit pending assets limit", userId);
        var point = PointData
            .Measurement("PendingAssetsFloodCheck")
            .Field("userId", userId.ToString())
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);
        RobloxInfluxDb.WritePointInBackground(point);
    }
    
    public static void ReportGlobalPendingAssetsFloodCheckReached(long userId)
    {
        LogFloodCheck("GlobalPendingAssetsFloodCheck", "user {0} hit global pending assets limit", userId);
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("GlobalPendingAssetsFloodCheck")
            .Field("userId", userId.ToString())
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
    
    public static void ReportMessageFloodCheckReached(long userId, long messageCount)
    {
        LogFloodCheck("MessageLocalFloodCheck", "user {0} hit limit at {1}", userId, messageCount);
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("MessageLocalFloodCheck")
            .Field("userId", userId.ToString())
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }

    public static void ReportGlobalMessageFloodCheckReached(long userId, long messageCount)
    {
        LogFloodCheck("MessageGlobalFloodCheck", "user {0} hit limit at {1}", userId, messageCount);
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("MessageGlobalFloodCheck")
            .Field("userId", userId.ToString())
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
    
    public static void ReportFollowingFloodCheckReached(long userId, long followingCount)
    {
        LogFloodCheck("FollowFloodCheck", "user {0} hit limit at {1}", userId, followingCount);
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("FollowFloodCheck")
            .Field("userId", userId.ToString())
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
    
    public static void ReportGlobalFollowingFloodCheckReached(long userId, long followingCount)
    {
        LogFloodCheck("FollowGlobalFloodCheck", "user {0} hit limit at {1}", userId, followingCount);
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("FollowGlobalFloodCheck")
            .Field("userId", userId.ToString())
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }

    public static void ReportFriendRequestFloodCheckReached(long userId, long requestCount)
    {
        LogFloodCheck("FriendRequestFloodCheck", "user {0} hit limit at {1}", userId, requestCount);
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("FriendRequestFloodCheck")
            .Field("userId", userId.ToString())
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }

    public static void ReportGlobalFriendRequestFloodCheckReached(long userId, long requestCount)
    {
        LogFloodCheck("FriendRequestGlobalFloodCheck", "user {0} hit limit at {1}", userId, requestCount);
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("FriendRequestGlobalFloodCheck")
            .Field("userId", userId.ToString())
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
    
    public static void ReportLoginFloodCheckReached(long requestCount)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("LoginFloodCheckReached")
            .Field("requestCount", requestCount)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
    
    public static void ReportLoginConcurrentLockHit()
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("LoginConcurrentLockHit")
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }

    public static async Task ReportUserSignUp()
    {
        await RobloxInfluxDb.WritePointAsync(PointData
            .Measurement("UserSignUp")
            .Timestamp(DateTime.UtcNow, WritePrecision.Ms)
        );
    }

    public static void ReportUserSignUpFromInvite()
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("UserJoinFromInvite")
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
    
    public static void ReportUserSignUpFromApplication()
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("UserJoinFromInvite")
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }

    public enum CaptchaFailureType
    {
        Login = 1,
    }
    
    public static void ReportCaptchaFailure(CaptchaFailureType type)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("CaptchaFailure")
            .Tag("type", type.ToString())
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
    
    public static void ReportUserLoginAttempt(bool wasSuccessful)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("UserLogin")
            .Tag("wasSuccessful", wasSuccessful ? "true" : "false")
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }

    public static void ReportApplicationDuplicateSocialUrl(string applicationId, string providedUrl, string originalUrl)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("ApplicationDuplicateSocialUrl")
            .Tag("applicationId", applicationId)
            .Field("providedUrl", providedUrl)
            .Field("originalUrl", originalUrl)
        );
    }
}