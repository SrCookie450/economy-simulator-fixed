using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Net.Http.Headers;
using Roblox.Libraries.EasyJwt;
using Roblox.Libraries.Password;
using Roblox.Logging;
using Roblox.Models.Sessions;
using Roblox.Services.App.FeatureFlags;
using Roblox.Website.Controllers;
using Roblox.Website.Lib;

namespace Roblox.Website.Middleware;

public class ApplicationGuardMiddleware
{
    private static string authorization { get; set; }
    public const string AuthorizationHeaderName = "rblx-authorization";
    public const string AuthorizationCookieName = "rblx-authorization";

    public static List<string> allowedUrls { get; } = new()
    {
        "/auth/captcha",
        "/auth/discord",
        "/auth/submit",
        "/auth/home",
        "/auth/privacy",
        "/auth/tos",
        "/auth/login",
        "/auth/password-reset",
        "/auth/contact",
        "/auth/account-deletion",
        "/auth/application",
        "/auth/signup",
        "/auth/ticket",
        "/auth/application-check",
        // razor public
        "/UnsecuredContent",
        // gs
        "/gs/activity",
        "/gs/ping",
        "/gs/delete",
        "/gs/shutdown",
        "/gs/players/report",
        "/gs/a",
        // other
        "/game/validate-machine",
        "/game/validateticket.ashx",
        "/game/get-join-script-debug",
        "/api/moderation/filtertext"
        // for forums or they don't work properly
    };

    public static void Configure(string authorizationString)
    {
        authorization = authorizationString;
    }

    public static string GetKey()
    {
        return authorization;
    }

    private RequestDelegate _next { get; set; }

    public ApplicationGuardMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    private bool IsAuthorized(HttpContext ctx)
    {
        if (ctx.Request.Headers.ContainsKey(AuthorizationHeaderName))
        {
            return ctx.Request.Headers[AuthorizationHeaderName].ToArray()[0] == authorization;
        }

        if (ctx.Request.Cookies.ContainsKey(AuthorizationCookieName))
        {
            // return ctx.Request.Cookies[AuthorizationCookieName]?.Contains(authorization) ?? false;
        }

        if (ctx.Items.ContainsKey(".ROBLOSECURITY"))
            return true;

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining|MethodImplOptions.AggressiveOptimization)]
    private static bool IsOldBrowser(string ua)
    {
        // this is mostly to stop bots using ancient user agents, which is (strangely) incredibly common
        const int minVersionChrome = 70; // todo: increase to 81
        // https://www.whatismybrowser.com/guides/the-latest-user-agent/chrome
        const int minVersionFirefox = 70; // todo: increase to 78
        // https://www.whatismybrowser.com/guides/the-latest-user-agent/firefox
        const int minVersionSafari = 602; // we actually use the WebKit version here
        // https://www.whatismybrowser.com/guides/the-latest-user-agent/safari
        if (ua.Contains("chrome/"))
        {
            for (var i = 0; i < minVersionChrome; i++)
            {
                if (ua.Contains("chrome/" + i + "."))
                    return true;
            }
        }
        else if (ua.Contains("firefox/"))
        {
            for (var i = 0; i < minVersionFirefox; i++)
            {
                if (ua.Contains("firefox/" + i + "."))
                    return true;
            }
        }
        else if (ua.Contains("safari/"))
        {
            for (var i = 0; i < minVersionSafari; i++)
            {
                if (ua.Contains("safari/" + i + "."))
                    return true;
            }
        }

        return false;
    }

    [SuppressMessage("ReSharper", "StringIndexOfIsCultureSpecific.1")]
    [MethodImpl(MethodImplOptions.AggressiveInlining|MethodImplOptions.AggressiveOptimization)]
    private bool IsUserAgentBlocked(string ua)
    {
        // note that this isn't really for blocking malicious actors, it's just for preventing search engines (and
        // similar services) from crawling our site
        ua = ua.ToLower().Trim();
        if (string.IsNullOrWhiteSpace(ua)) return true;
        // Google Crawlers
        // please keep this up-to-date with https://developers.google.com/search/docs/advanced/crawling/overview-google-crawlers
        if (ua.IndexOf("apis-google") != -1) return true;
        if (ua.IndexOf("mediapartners-google") != -1) return true;
        if (ua.IndexOf("adsbot-google") != -1) return true; // adsbot-google, adsbot-google-mobile, adsbot-google-Mobile-Apps
        if (ua.IndexOf("googlebot") != -1) return true; // Googlebot, Googlebot-Image, Googlebot-News, Googlebot-Video
        if (ua.IndexOf("feedfetcher-google") != -1) return true;
        if (ua.IndexOf("google-read-aloud") != -1) return true;
        if (ua.IndexOf("duplexweb-google") != -1) return true;
        if (ua.IndexOf("storebot-google") != -1) return true;
        if (ua.IndexOf("google-site-verification") != -1) return true; // https://www.google.com/webmasters/tools/verification/google-site-verification.html
        if (ua == "google") return true; // sometimes the ua is literally just "google"? wtf
        // todo: do we block "Google Favicon"?
        // todo: do we block "googleweblight"?

        // Bing Crawlers
        // please keep this up-to-date with https://www.bing.com/webmasters/help/which-crawlers-does-bing-use-8c184ec0
        if (ua.IndexOf("bingbot") != -1) return true;
        if (ua.IndexOf("adidxbot") != -1) return true;
        if (ua.IndexOf("bingpreview") != -1) return true;

        // Yahoo Crawlers
        // please keep this up-to-date with https://help.yahoo.com/kb/SLN22600.html
        if (ua.IndexOf("yahoo! slurp") != -1) return true;

        // Facebook (meta) Crawlers
        // please keep this up-to-date with https://developers.facebook.com/docs/sharing/webmasters/crawler/
        if (ua.IndexOf("facebookexternalhit") != -1) return true;

        // Other crawlers
        if (ua.IndexOf("qwantify") != -1) return true;
        if (ua.IndexOf("duckduckgo") != -1) return true;
        
        // Old browsers
        // Even if they were legitimate users, they probably wouldn't be able to use the site due to the ssl certs being
        // too new, and even if they could visit the site, it wouldn't even load properly.
        if (IsOldBrowser(ua)) return true;
        if (ua == "chrome") return true; // todo: what is this?
        if (ua == "firefox") return true; // todo: what is this?
        if (ua == "safari") return true; // todo: what is this?
        if (ua == "opera") return true; // todo: what is this?

        // Misc
        // From https://developers.whatismybrowser.com/useragents/explore/software_type_specific/crawler/
        if (ua.IndexOf("baiduspider") != -1) return true;
        if (ua.IndexOf("mj12bot") != -1) return true; // https://majestic.com/
        if (ua.IndexOf("megaindex") != -1) return true;
        if (ua.IndexOf("ahrefsbot") != -1) return true;
        if (ua.IndexOf("semrushbot") != -1) return true;
        if (ua.IndexOf("dotbot") != -1) return true;
        if (ua.IndexOf("jobboersebot") != -1) return true;
        if (ua.IndexOf("yandexbot") != -1) return true;
        if (ua.IndexOf("yandex.com") != -1) return true;
        if (ua.IndexOf("developers.google.com") != -1) return true; // https://developers.whatismybrowser.com/useragents/parse/464220google-snippet-fetcher
        if (ua.IndexOf("msnbot") != -1) return true; // msn is still around???
        if (ua.IndexOf("seoscanners.net") != -1) return true;
        if (ua.IndexOf("seokicks") != -1) return true;
        if (ua.IndexOf("petalbot") != -1) return true;
        if (ua.IndexOf("ia_archiver") != -1) return true; // archive.org
        if (ua.IndexOf("censys") != -1) return true;
        if (ua.IndexOf("paloaltonetworks") != -1) return true;
        if (ua.IndexOf("alittle client") != -1) return true;
        if (ua.IndexOf("webmeup-crawler.com") != -1) return true;
        if (ua.IndexOf("blexbot") != -1) return true;
        if (ua.IndexOf("turnitinbot") != -1) return true; // http://www.turnitin.com/robot/crawlerinfo.html
        if (ua.IndexOf("npbot") != -1) return true; // http://www.nameprotect.com/botinfo.html
        if (ua.IndexOf("slysearch") != -1) return true; // http://www.slysearch.com/
        if (ua.IndexOf("spaziodati.eu") != -1) return true;
        if (ua.IndexOf("ezgif.com") != -1) return true;
        if (ua.IndexOf("archive.org") != -1) return true;
        if (ua.IndexOf("iframely") != -1) return true;
        if (ua.IndexOf("googlesites") != -1) return true;
        if (ua.IndexOf("comscore.com") != -1) return true; // https://www.comscore.com/Web-Crawler
        if (ua.IndexOf("proximic.com") != -1) return true; // http://www.proximic.com/info/spider.php
        if (ua.IndexOf("opengraph.io") != -1) return true; // https://opengraph.io/
        if (ua.IndexOf("roblox.com") != -1) return true; // todo: what is this?
        if (ua.IndexOf("seznambot") != -1) return true; // https://napoveda.seznam.cz/en/seznamcz-web-search/
        if (ua.IndexOf("headline.com") != -1) return true; // https://www.headline.com/
        if (ua.IndexOf("ev-crawler") != -1) return true; // https://www.headline.com/
        if (ua.IndexOf("crawler4j") != -1) return true; // https://crawler4j.github.io/crawler4j/
        if (ua.IndexOf("api.slack.com") != -1) return true; // https://api.slack.com/robots.txt
        if (ua.IndexOf("slackbot") != -1) return true; // https://api.slack.com/robots.txt
        if (ua.IndexOf("slack-img") != -1) return true; // https://api.slack.com/robots.txt
        if (ua.IndexOf("slack-screenshot") != -1) return true; // https://api.slack.com/robots.txt
        if (ua.IndexOf("slack-ss") != -1) return true; // https://api.slack.com/robots.txt
        // languages (if you get blocked, do NOT uncomment these, just change your UA to something descriptive
        // (e.g. "AvatarRender/1.0")
        if (ua.IndexOf("python-requests") != -1) return true;
        if (ua.IndexOf("go-http-client") != -1) return true;
        if (ua.IndexOf("axios") != -1) return true;
        if (ua.IndexOf("node-fetch") != -1) return true;
        if (ua.IndexOf("node-request") != -1) return true;
        if (ua.IndexOf("node-http") != -1) return true;
        if (ua.IndexOf("node-https") != -1) return true;
        if (ua.IndexOf("grequests") != -1) return true;
        if (ua.IndexOf("http-client") != -1) return true;
        if (ua.IndexOf("github.com") != -1) return true; // e.g. "github.com/sindresorhus/got"
        if (ua.IndexOf("gitlab.com") != -1) return true;
        if (ua.IndexOf("bitbucket.org") != -1) return true;
        if (ua.IndexOf("bitbucket.com") != -1) return true;
        if (ua.IndexOf("githubusercontent.com") != -1) return true;
        if (ua.IndexOf("github.io") != -1) return true;
        if (ua == "ruby") return true;
        if (ua.IndexOf("test certificate info") != -1) return true;
        if (ua == "wp_is_mobile") return true; // no clue what this is
        if (ua.IndexOf("curl/") != -1) return true;
        if (ua.IndexOf("wget/") != -1) return true;
        if (ua.IndexOf("well-known.dev") != -1) return true;
        if (ua == "aids") return true; // ?

        return false;
    }

    private readonly string[] allowedPathsForBlockUserAgents = new[]
    {
        "",
        "/auth/home",
        "/auth/captcha",
    };

    private async Task Redirect(HttpContext ctx, string dest)
    {
        ctx.Response.StatusCode = 302;
        ctx.Response.Headers.Location = "/auth/home";
        await ctx.Response.WriteAsync("Object moved to <a href=\""+dest+"\">here</a>.");
    }
    
    public async Task InvokeAsync(HttpContext ctx)
    {
        var appGuardTimer = new MiddlewareTimer(ctx, "AppGuard");
        
        var normalizedPath = ctx.Request.Path.Value?.ToLower() ?? "";
        if (normalizedPath.EndsWith("/"))
        {
            normalizedPath = normalizedPath.Substring(0, normalizedPath.Length - 1);
        }

        if (normalizedPath == "/robots.txt")
        {
            var created = DateTime.UtcNow.ToString("O");
            var disallow = new List<string>()
            {
                "/My/*",
                "/Games/*",
                "/Users/*",
                "/Catalog",
                "/Catalog/*",
                "/Forum",
                "/Forum/*",
                "/Internal/*",
            };
            var newItems = new List<string>();
            foreach (var item in disallow)
            {
                newItems.Add(item.ToLower());
            }
            newItems.ForEach(v => disallow.Add(v));
            ctx.Response.Headers.ContentType = "text/plain";
            ctx.Response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = true,
                MaxAge = TimeSpan.FromHours(24),
            }.ToString();
            await ctx.Response.WriteAsync("user-agent: *\n\n" + string.Join("\n", disallow.Select(c => "disallow: " + c)) + "\n\n#" + created);
            return;
        }

        var uaTimer = new MiddlewareTimer(ctx, "ua");
        var ua = ctx.Request.Headers["user-agent"].ToString();
        var uaBlocked = IsUserAgentBlocked(ua);
        var bypassOk = false;
        var bypassAllowedForPath = allowedPathsForBlockUserAgents.Contains(normalizedPath);
        
        if (uaBlocked && !bypassAllowedForPath)
        {
            var uaBypassWatch = new Stopwatch();
            uaBypassWatch.Start();
            
            if (ctx.Request.Cookies.TryGetValue("uabypass1", out var cookieBypass) && !string.IsNullOrWhiteSpace(cookieBypass))
            {
                var deleteCookie = false;
                try
                {
                    var jwtService = new EasyJwt();
                    var result =
                        jwtService.DecodeJwt<UserAgentBypass>(cookieBypass, Roblox.Configuration.UserAgentBypassSecret);
                    if (result.createdAt > DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)) &&
                        result.userAgent == ctx.Request.Headers.UserAgent)
                    {
                        var ipHash = ControllerBase.GetIP(ControllerBase.GetRequesterIpRaw(ctx), result.GetSalt());
                        if (result.ipAddress == ipHash)
                        {
                            uaBlocked = false;
                            bypassOk = true;
                        }
                        else
                        {
                            deleteCookie = true;
                        }
                    }
                    else
                    {
                        deleteCookie = true;
                    }
                    
                }
                catch (Exception e)
                {
                    deleteCookie = true;
                    Writer.Info(LogGroup.AbuseDetection, "Error while decoding UA bypass: " + e.Message);
                }

                if (deleteCookie)
                    ctx.Response.Cookies.Delete("uabypass1");
            }
            uaBypassWatch.Stop();
            Writer.Info(LogGroup.AbuseDetection, "took {0}ms to parse ua bypass cookie", uaBypassWatch.ElapsedMilliseconds);
        }
        if (uaBlocked && !bypassAllowedForPath)
        {
            ctx.Response.StatusCode = 302;
            ctx.Response.Headers.ContentType = "text/html; charset=utf-8";
            ctx.Response.Headers.Location = "/auth/captcha";
            await ctx.Response.WriteAsync("Please click <a href=\"/auth/captcha\">here</a> to continue.");
            Roblox.Metrics.ApplicationGuardMetrics.ReportBlockedUserAgent(ua);
            return;
        }
        uaTimer.Stop();

        if (!uaBlocked && !bypassOk && !bypassAllowedForPath)
            Roblox.Metrics.ApplicationGuardMetrics.ReportAllowedUserAgent(ua);

        var authTimer = new MiddlewareTimer(ctx, "a");
        var isAuthorized = IsAuthorized(ctx);
        authTimer.Stop();

        if (isAuthorized || allowedUrls.Contains(normalizedPath) /*||
            RobloxPlayerDataRegex.IsMatch(normalizedPath)*/)
            {
                appGuardTimer.Stop();
                await _next(ctx);
                return;
            }
            
            // If not blocked
            if (FeatureFlags.IsDisabled(FeatureFlag.AllowAccessToAllRequests))
            {
                await Redirect(ctx, "/auth/home");
                return;
            }
            // Otherwise, allow (almost) all
            if (normalizedPath == "")
            {
                await Redirect(ctx, "/auth/home");
                return;
            }
            appGuardTimer.Stop();
            await _next(ctx);
        }
}

public static class ApplicationGuardMiddlewareExtensions
{
    public static IApplicationBuilder UseApplicationGuardMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApplicationGuardMiddleware>();
    }
}