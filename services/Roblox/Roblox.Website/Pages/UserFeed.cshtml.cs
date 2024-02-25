using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Dto.Users;
using Roblox.Models.Assets;
using Roblox.Models.Sessions;
using Roblox.Services.App.FeatureFlags;
using Roblox.Website.Controllers;
using Roblox.Website.Pages;

namespace Roblox.Website.Views;

public class UserFeed : RobloxPageModel
{
    public IEnumerable<FeedEntry>? feedList { get; set; }

    public async Task OnGet()
    {
        FeatureFlags.FeatureCheck(FeatureFlag.UserFeedEnabled);
        if (userSession == null) return;
        
        const int feedLimit = 100;
        const int followerLimit = 1000;
        var feedCacheKey = $"UserFeedV1.2:{userSession.userId}";
#if RELEASE
        var cacheResult = await Services.Cache.distributed.StringGetAsync(feedCacheKey);
        if (cacheResult != null)
        {
            feedList = JsonSerializer.Deserialize<List<FeedEntry>>(cacheResult);
            return;
        }
#endif
        var feeds = new List<FeedEntry>();

        var ids = new List<long>();
        ids.AddRange((await services.friends.GetFriends(userSession.userId)).Select(c => c.id));
        ids.AddRange(await services.friends.GetFollowingIds(userSession.userId, followerLimit));
        if (ids.Count != 0)
        {
            feeds = (await services.users.MultiGetLatestStatus(ids.Distinct(), feedLimit)).ToList();
        }

        var groups = (await services.groups.GetAllRolesForUser(userSession.userId)).ToList();
        if (groups.Count != 0)
        {
            var groupFeed = await services.groups.MultiGetGroupStatus(groups.Select(c => c.groupId), feedLimit);
            foreach (var entry in groupFeed)
            {
                feeds.Add(entry);
            }
        }
        
        feeds.Sort((a, b) => a.created > b.created ? -1 : a.created < b.created ? 1 : 0);
        feeds = feeds.Where(a => !string.IsNullOrWhiteSpace(a.content)).Where((a, idx) =>
        {
            // var previousEntry = idx > 0 ? feeds[idx - 1] : null;
            // var nextEntry = idx < feeds.Count - 1 ? feeds[idx + 1] : null;
            var nextEntry = feeds.Where((b, j) =>
            {
                if (a.group != null)
                    return a.group.id == b.group?.id && j > idx;
                return a.user.id == b.user.id && j > idx;
            }).FirstOrDefault();

            var ok = true;
            if (nextEntry != null)
            {
                ok = a.content.ToLowerInvariant() != nextEntry.content.ToLowerInvariant();
            }

            if (ok)
            {
                var previousEntry = feeds.Where((b, j) =>
                {
                    if (a.group != null)
                        return a.group.id == b.group?.id && j < idx;
                    return a.user.id == b.user.id && j < idx;
                }).FirstOrDefault();
                if (previousEntry != null)
                {
                    // People do stuff like this:
                    // Post2: "Hey guys what's up I'm doing great"
                    // Post1: "Hey guys what's up"
                    // (i.e. they make a post with the same prefix twice, they just add something to the latest post).
                    // this is our way of de-duping that
                    ok = !previousEntry.content.ToLowerInvariant().StartsWith(a.content.ToLowerInvariant());
                }
            }

            return ok;
        }).ToList();
        feedList = feeds.Take(feedLimit);
        await Services.Cache.distributed.StringSetAsync(feedCacheKey, JsonSerializer.Serialize(feedList),
            TimeSpan.FromHours(2));
    }
}