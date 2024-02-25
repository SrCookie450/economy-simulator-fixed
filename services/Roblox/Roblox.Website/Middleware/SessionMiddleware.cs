using System.Text.Json;
using System.Text.Json.Serialization;
using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using Microsoft.AspNetCore.Http.Extensions;
using Roblox.Dto.Users;
using Roblox.Models.Sessions;
using Roblox.Models.Users;
using Roblox.Services;
using Roblox.Services.Exceptions;
using Roblox.Website.Controllers;
using Roblox.Website.Filters;
using Roblox.Website.Lib;
using ServiceProvider = Roblox.Services.ServiceProvider;

namespace Roblox.Website.Middleware;

public class JwtEntry
{
    public string sessionId { get; set; }
    public long createdAt { get; set; }
}

public class SessionMiddleware
{
    private RequestDelegate _next;
    public const string CookieName = ".ROBLOSECURITY";
    // JWT Config
    private static readonly IJwtAlgorithm Algorithm = new HMACSHA512Algorithm();
    private static readonly IJsonSerializer Serializer = new JsonNetSerializer();
    private static readonly IBase64UrlEncoder UrlEncoder = new JwtBase64UrlEncoder();
    private static readonly IDateTimeProvider DateTimeProvider = new UtcDateTimeProvider();
    private static readonly IJwtValidator Validator = new JwtValidator(Serializer, DateTimeProvider);

    private static readonly IJwtEncoder Encoder = new JwtEncoder(Algorithm, Serializer, UrlEncoder);
    private static readonly IJwtDecoder Decoder = new JwtDecoder(Serializer, Validator, UrlEncoder, Algorithm);

    private static string cookieJwtKey { get; set; }

    public static void Configure(string newJwtKey)
    {
        if (cookieJwtKey != null) throw new Exception("Already configured");
        cookieJwtKey = newJwtKey;
    }


    public SessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public static string CreateJwt<T>(T obj)
    {
        var token = Encoder.Encode(obj, cookieJwtKey);
        if (token == null) throw new NullReferenceException();
        return token;
    }

    public static T DecodeJwt<T>(string token)
    {
        var json = Decoder.Decode(token, cookieJwtKey, verify: true);
        if (json == null) throw new NullReferenceException();
        var result = JsonSerializer.Deserialize<T>(json);
        if (result == null) throw new NullReferenceException();
        return result;
    }

    private async Task OnBadSession(HttpContext ctx)
    {
        ctx.Response.Cookies.Delete(CookieName);
        await _next(ctx);
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        var authTimer = new MiddlewareTimer(ctx, "au");
        var currentPath = ctx.Request.Path.ToString().ToLower();
        try
        {
            if (ctx.Request.Cookies.ContainsKey(CookieName))
            {
                var cookie = ctx.Request.Cookies[CookieName];
                if (string.IsNullOrWhiteSpace(cookie))
                {
                    authTimer.Stop();
                    await OnBadSession(ctx);
                    return;
                }
                
                var decodedResult = DecodeJwt<JwtEntry>(cookie);
                if (!string.IsNullOrEmpty(decodedResult.sessionId))
                {
                    using var users = ServiceProvider.GetOrCreate<UsersService>();
                    using var accountInformation = ServiceProvider.GetOrCreate<AccountInformationService>();
                    
                    UserInfo userInfo;
                    try
                    {
                        var sessResult = await users.GetSessionById(decodedResult.sessionId);
                        userInfo = await users.GetUserById(sessResult.userId);
                    }
                    catch (RecordNotFoundException)
                    {
                        authTimer.Stop();
                        await OnBadSession(ctx);
                        return;
                    }
                    if (userInfo.accountStatus is AccountStatus.Forgotten or AccountStatus.MustValidateEmail)
                    {
                        authTimer.Stop();
                        await OnBadSession(ctx);
                        return; // Don't add session - pretend user doesn't exist
                    }

                    ctx.Items[CookieName] = new UserSession(userInfo.userId, userInfo.username, userInfo.created, userInfo.accountStatus, 0, false, decodedResult.sessionId);

                    if (userInfo.accountStatus is AccountStatus.Suppressed or AccountStatus.Poisoned
                        or AccountStatus.Deleted)
                    {
                        // allow access to other auth pages, as well as "discord" url
                        if (!currentPath.StartsWith("/auth/"))
                        {
                            authTimer.Stop();
                            ctx.Response.StatusCode = 302;
                            ctx.Response.Headers.Add("location", "/auth/notapproved");
                            return;
                        }
                    }
                    // Check if user filled out new app
                    var appStatus = await users.IsUserApproved(userInfo.userId);
                    if (!appStatus && !userInfo.isAdmin && !userInfo.isModerator && !StaffFilter.IsOwner(userInfo.userId))
                    {
                        if (!currentPath.StartsWith("/auth/"))
                        {
                            authTimer.Stop();
                            ctx.Response.StatusCode = 302;
                            ctx.Response.Headers.Add("location", "/auth/application");
                            return;
                        }
                    }

                    // robux increment
                    if (!currentPath.StartsWith("/thumbs/") && !currentPath.StartsWith("/images/"))
                    {
                        await users.EarnDailyRobuxNoVirusNoScamHindiSubtitles(userInfo.userId, await StaffFilter.IsStaff(userInfo.userId));
                        await users.EarnDailyTickets(userInfo.userId);
                        if (users.TrySetOnlineTimeUpdated(userInfo.userId))
                        {
                            await users.UpdateOnlineStatus(userInfo.userId);
                        }
                    }

                    if (currentPath == "/")
                    {
                        // Redirect to homepage
                        ctx.Response.StatusCode = 302;
                        ctx.Response.Headers.Add("location", "/home");
                        return;
                    }
                    authTimer.Stop();
                }
            }
        }
        catch (System.Exception e) when (e is InvalidTokenPartsException or NullReferenceException or FormatException)
        {
            ctx.Response.Cookies.Delete(CookieName);
        }
        await _next(ctx);
    }
}

public static class SessionMiddlewareExtensions
{
    public static IApplicationBuilder UseRobloxSessionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionMiddleware>();
    }
}