using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Roblox.Libraries.Assets;
using Roblox.Libraries.RemoteView;
using Roblox.Models.Assets;
using Roblox.Models.Users;

#pragma warning disable CS8620

namespace Roblox.Website.Controllers;

public class ViewUserInfo
{
    public long userId { get; set; }
    public string username { get; set; }
    public DateTime created { get; set; }
    public string theme { get; set; }
    public AccountStatus status { get; set; }
    public bool isModerator { get; set; }
    public bool isAdmin { get; set; }
    public int sessionKey { get; set; }

    public bool isImpersonating => false;

    public bool isGame => false;
    /*
        userId: number;
        username: string;
        created: string;
        theme: string;
        status: AccountStatus;
        isModerator: boolean;
        isAdmin: boolean;
        sessionKey: number;
        isImpersonating: boolean;
        isGame?: boolean;
    */
}

[ApiController]
[Route("/")]
public class WebController2021 : ControllerBase
{
    [HttpGet("/apisite/ecsv2/www/e.png")]
    public void ReportEcsAnalticsV2()
    {
        
    }
    
    private async Task<IActionResult> GetPage(string viewName, IEnumerable<dynamic>? arguments = null)
    {
        var newArgs = new List<dynamic>();
        var v = new ViewUserInfo()
        {
            userId = userSession.userId,
            username = userSession.username,
            created = userSession.created,
            isAdmin = false, // obsolete
            isModerator = false, // obsolete
            sessionKey = userSession.sessionKey,
            status = userSession.accountStatus,
            theme = ThemeTypes.Light.ToString(),
        };
        newArgs.Add(v);
        if (arguments != null)
        {
            foreach (var item in arguments)
            {
                newArgs.Add(item);
            }
        }
        var result = await RemoteView.GetView(viewName, newArgs);
        return Content(result, "text/html");
    }
    
    public static dynamic ToDynamic<T>(T obj)
    {
        IDictionary<string, object> expando = new ExpandoObject();

        foreach (var propertyInfo in typeof(T).GetProperties())
        {
            var currentValue = propertyInfo.GetValue(obj);
            expando.Add(propertyInfo.Name, currentValue);
        }
        return expando as ExpandoObject;
    }

    [HttpGet("/home")]
    public async Task<IActionResult> GetHome()
    {
        return await GetPage("dashboard");
    }
    
    [HttpGet("/trades")]
    public async Task<IActionResult> GetTrades()
    {
        return await GetPage("trades");
    }
        
    [HttpGet("/users/{userId:long}/trade")]
    public async Task<IActionResult> GetTradeWithUser(long userId)
    {
        return await GetPage("tradeWithUser", new List<dynamic>()
        {
            new
            {
                userId = userId,
            },
        });
    }

    [HttpGet("/users/{userId:long}/profile")]
    public async Task<IActionResult> GetUserProfile(long userId)
    {
        var info = await services.users.GetUserById(userId);
        if (info.IsDeleted())
        {
            return Redirect("/404");
        }

        var wornAssets = await services.assets.MultiGetInfoById(await services.avatar.GetWornAssets(userId));
        var friendStatus = (await services.friends.MultiGetFriendshipStatus(userSession?.userId ?? 0, new[] {userId}))
            .First();
        var friends = await services.friends.GetFriends(userId);
        var previousNames = await services.users.GetPreviousUsernames(userId);
        var status = await services.users.GetUserStatus(userId);
        var onlineStatus = (await services.users.MultiGetPresence(new[] {userId})).First();
        var inventoryVisible = (await services.inventory.MultiCanViewInventory(new[] {userId}, userSession?.userId ?? 0)).First();
        var followersCount = await services.friends.CountFollowers(userId);
        var followingsCount = await services.friends.CountFollowings(userId);
        var followStatus =
            userSession != null && (await services.friends.IsOneFollowingTwo(userSession.userId, userId)) || false;
        var playingGameDetails = onlineStatus.placeId != null
            ? await services.assets.GetAssetCatalogInfo((long)onlineStatus.placeId)
            : null;
        var createdPlaces = (await services.games.GetGamesForType(CreatorType.User, userId, 100, 0, "asc", "all")).Select(c => ToDynamic(c)).ToList();
        foreach (var place in createdPlaces)
        {
            var id = (long) place.rootPlace.id;
            place.playerCount = await services.games.GetPlayerCount(id);
            place.visitCount = await services.games.GetVisitCount(id);
        }
        return await GetPage("userProfile", new List<dynamic>()
        {
            new
            {
                userId = userId,
                username = info.username,
                description = info.description,
                imageUrl = "/thumbs/avatar.ashx?userId=" + userId,
                wornAssets = wornAssets,
                created = info.created,
                friendshipStatus = friendStatus.status,
                friends = friends,
                previousUsernames = previousNames.Select(c => c.username),
                status = status.status == null ? null : status,
                onlineAt = onlineStatus.lastOnline,
                followers = followersCount,
                following = followingsCount,
                canViewInventory = inventoryVisible.canView,
                isFollowing = followStatus,
                gameData = onlineStatus.gameId != null ? onlineStatus : null,
                gameDetails = playingGameDetails,
                places = createdPlaces,
            },
        });
    }
    
        
    [HttpGet("/search/groups")]
    [HttpGet("/groups/search")]
    public async Task<IActionResult> SearchGroups()
    {
        return await GetPage("groupSearch");
    }

    [HttpGet("search/users")]
    public async Task<IActionResult> SearchUsers(string keyword)
    {
        if (string.IsNullOrEmpty(keyword) || keyword.Length >= 32 || keyword.IndexOf(">") != -1 || keyword.IndexOf("<") != -1) 
            return Content("Invalid search keyword");
        return await GetPage("userSearch", new List<dynamic>() {keyword});
    }
    
    [HttpGet("/groups/create")]
    public async Task<IActionResult> CreateGroup()
    {
        return await GetPage("groupCreate");
    }

    [HttpGet("groups")]
    [HttpGet("my/groups")]
    public async Task<IActionResult> GroupsRedirect()
    {
        var hasGroups = (await services.groups.GetAllRolesForUser(userSession.userId)).ToList();
        if (hasGroups.Count > 0)
        {
            return Redirect("/groups/" + hasGroups.First().id + "/" +
                            UrlUtilities.ConvertToSeoName(hasGroups.First().name));
        }

        return Redirect("/groups/search");
    }

    [HttpGet("groups/{groupId:long}/{name}")]
    public async Task<IActionResult> GetGroup(long groupId, string name)
    {
        var details = await services.groups.GetGroupById(groupId);
        var expectedName = UrlUtilities.ConvertToSeoName(details.name);
        if (expectedName != name) return Redirect("/groups/" + groupId + "/" + expectedName);
        var thumb = await services.thumbnails.GetGroupIcons(new[] {groupId});
        return await GetPage("group", new List<dynamic>()
        {
            details,
            thumb.First()?.imageUrl,
        });
    }

    [HttpGet("groups/configure")]
    public async Task<IActionResult> ConfigureGroup(long id)
    {
        var details = await services.groups.GetGroupById(id);
        return await GetPage("groupConfigure", new List<dynamic>()
        {
            details,
        });
    }

    [HttpGet("/catalog/configure")]
    public async Task<IActionResult> ConfigureAsset(long id)
    {
        var details = await services.assets.GetAssetCatalogInfo(id);
        return await GetPage("configureAsset", new List<dynamic>()
        {
            new
            {
                assetId = details.id,
                assetTypeId = (int) details.assetType,
            }
        });
    }

    [HttpGet("users/{userId:long}/inventory")]
    public async Task<IActionResult> InventoryPage(long userId)
    {
        var data = await services.users.GetUserById(userId);
        return await GetPage("userInventory", new List<dynamic>()
        {
            new
            {
                username = data.username,
                userId = data.userId,
            },
        });
    }
    
    [HttpGet("users/{userId:long}/friends")]
    public async Task<IActionResult> FriendsPage(long userId)
    {
        var data = await services.users.GetUserById(userId);
        return await GetPage("userFriends", new List<dynamic>()
        {
            new
            {
                username = data.username,
                userId = data.userId,
            },
        });
    }
    
    [HttpGet("users/friends")]
    public async Task<IActionResult> MyFriendsPage()
    {
        return await GetPage("myFriends", new List<dynamic>()
        {
            new
            {
                username = userSession.username,
                userId = userSession.userId,
            },
        });
    }

    [HttpGet("my/messages")]
    public async Task<IActionResult> MyMessages()
    {
        // return await GetPage("myMessages");
        return Content("Messages are disabled on 2021 due to possible xss issues. I'll hopefully fix this soon.");
    }
    
    [HttpGet("games")]
    public async Task<IActionResult> GamesPage()
    {
        return await GetPage("games");
    }

    [HttpGet("games/refer")]
    public async Task<IActionResult> RedirectToGameDetailsPage(long PlaceId)
    {
        return Redirect("/games/" + PlaceId + "/--");
    }

    [HttpGet("games/{placeId:long}/{placeName}")]
    public async Task<IActionResult> GetGameDetailsPage(long placeId, string placeName)
    {
        var details = ToDynamic((await services.games.MultiGetPlaceDetails(new[] {placeId})).First());
        var expectedName = UrlUtilities.ConvertToSeoName(details.name);
        if (expectedName != placeName)
        {
            return Redirect("/games/" + placeId + "/" + expectedName);
        }
        details.playerCount = await services.games.GetPlayerCount(placeId);
        details.favoriteCount = 0;
        details.visitCount = await services.games.GetVisitCount(placeId);
        return await GetPage("gameDetails", new []{details});
    }

    [HttpGet("/games/votingservice/{placeId}")]
    public IActionResult GetVotingService()
    {
        return Content("<!-- TODO -->", "text/html");
    }
    
    [HttpGet("transactions")]
    public async Task<IActionResult> TransactionsPage()
    {
        return await GetPage("transactions");
    }
    
    [HttpGet("my/account")]
    public async Task<IActionResult> MySettingsPage()
    {
        return await GetPage("settings");
    }
    
    [HttpGet("catalog")]
    public async Task<IActionResult> Catalog()
    {
        return await GetPage("catalog");
    }

    [HttpGet("catalog/{assetId:long}")]
    public async Task<IActionResult> RedirectToCatalogDetailsPage(long assetId)
    {
        var details = await services.assets.GetAssetCatalogInfo(assetId);
        return Redirect("/catalog/" + assetId + "/" + UrlUtilities.ConvertToSeoName(details.name));
    }
    
    [HttpGet("catalog/{assetId:long}/{assetName}")]
    public async Task<IActionResult> GetCatalogPage(long assetId, string assetName)
    {
        var details = await services.assets.GetAssetCatalogInfo(assetId);
        var expectedName = UrlUtilities.ConvertToSeoName(details.name);
        if (expectedName != assetName)
        {
            return Redirect("/catalog/" + assetId + "/" + expectedName);
        }

        var marketData = await services.assets.GetProductForAsset(assetId);
        var owned = await services.users.GetUserAssets(userSession.userId, assetId);
        var pins = await services.inventory.GetCollections(userSession.userId);
        var comments = await services.assets.AreCommentsEnabled(assetId);

        var balance = await services.economy.GetUserRobux(userSession.userId);
        return await GetPage("catalogItem", new List<dynamic>()
        {
            balance,
            new
            {
                type = details.assetType.ToString(),
                genres = details.genres.Select(c => c.ToString()),
                creator = new
                {
                    id = details.creatorTargetId,
                    name = details.creatorName,
                    type = details.creatorType,
                },
                price = details.price,
                assetId = details.id,
                name = details.name,
                description = details.description,
                isLimited = details.itemRestrictions.Contains("Limited"),
                isLimitedU = details.itemRestrictions.Contains("LimitedUnique"),
                isForSale = details.isForSale,
                copiesTotal = marketData.serialCount,
                copiesAvailable = marketData.serialCount != null ? marketData.serialCount - details.saleCount : null,
                copiesOwnedByRequester = owned,
                bestPrice = details.lowestSellerData?.price,
                bestPriceData = details.lowestSellerData,
                offSaleDeadline = marketData.offsaleAt,
                isPinned = pins.Contains(assetId),
                assetTypeId = (int)details.assetType,
                commentsEnabled = comments,
            },
        });
    }
    
    [HttpGet("my/avatar")]
    public async Task<IActionResult> MyAvatar()
    {
        return await GetPage("myAvatar");
    }
}