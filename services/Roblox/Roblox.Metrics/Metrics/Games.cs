using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace Roblox.Metrics;

public static class GameMetrics
{
    public static void ReportFloodCheckForVoteShort(long userId, long placeId)
    {
         RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("FloodCheckForGameVoteShort")
            .Field("placeId", placeId)
            .Field("userId", userId)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
    
    public static void ReportFloodCheckForVoteLong(long userId, long placeId)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("FloodCheckForGameVoteLong")
            .Field("placeId", placeId)
            .Field("userId", userId)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }

    public static void ReportFloodCheckForAsset(long placeId)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("FloodCheckForGameVoteAsset")
            .Field("placeId", placeId)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
    
    public static async Task ReportGameJoinAttempt(long placeId)
    {
        await RobloxInfluxDb.WritePointAsync(PointData
            .Measurement("GameJoin_Attempt")
            .Field("placeId", placeId)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
    
    public static async Task ReportGameJoinPlaceLauncherReturned(long placeId)
    {
        await RobloxInfluxDb.WritePointAsync(PointData
            .Measurement("GameJoin_PlaceLauncherSuccess")
            .Field("placeId", placeId)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
    
    public static async Task ReportGameJoinSuccess(long placeId)
    {
        await RobloxInfluxDb.WritePointAsync(PointData
            .Measurement("GameJoin_Success")
            .Field("placeId", placeId)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }

    public static async Task ReportRccAuthorizationFailure(string rawUrl, string providedAuthorization, string rawIpAddress)
    {
        // Raw IP is so we can track down the server that is running the wrong binary or config or whatever.
        // It is extremely unlikely an unsuspecting user will reach this endpoint.
        await RobloxInfluxDb.WritePointAsync(PointData
            .Measurement("RccAuthorizationFailure")
            .Field("url", rawUrl)
            .Field("ipAddress", rawIpAddress)
            // .Field("providedAuthorization", providedAuthorization)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns)
        );
    }

    public static void ReportTicketErrorUserIdNotMatchingTicket(string rawTicket, long ticketUserId, long providedUserId)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
                .Measurement("TicketErrorUserIdNotMatchingTicket")
                .Field("ticket", rawTicket)
                .Field("ticketUserId", ticketUserId)
                .Field("providedUserId", providedUserId)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ns)
            // .Field("providedAuthorization", providedAuthorization)
        );
    }
    
    public static void ReportTimeToGetGameServerInfo(string serverIp, string serverPort, long durationInMs)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("TimeToGetGameServerInfo")
            .Field("serverIp", serverIp)
            .Field("serverPort", serverPort)
            .Field("durationInMilliseconds", durationInMs)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
    
    public static void ReportTimeToStartGameServer(string serverIp, string serverPort, long durationInMs)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("TimeToStartGameServer")
            .Field("serverIp", serverIp)
            .Field("serverPort", serverPort)
            .Field("durationInMilliseconds", durationInMs)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }
    
    public static void ReportServerShutdownWithoutDatabaseEntry(string serverIp, long placeId)
    {
        RobloxInfluxDb.WritePointInBackground(PointData
            .Measurement("ServerShutdownWithoutDatabaseEntry")
            .Field("serverIp", serverIp)
            .Field("placeId", placeId)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns));
    }

}