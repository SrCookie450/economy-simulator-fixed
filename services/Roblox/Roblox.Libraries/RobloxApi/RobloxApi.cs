using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Roblox.Logging;
// ReSharper disable InconsistentNaming

namespace Roblox.Libraries.RobloxApi;

public class ProductDataResponse
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Models.Assets.Type? AssetTypeId { get; set; }
    public bool? IsLimited { get; set; }
    public bool? IsLimitedUnique { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? Updated { get; set; }
}

public class AssetDeliveryResponse
{
    public string? location { get; set; }
}

public class AssetDeliveryEntry
{
    public string? assetFormat { get; set; }
    public string? location { get; set; }
}

public class AssetDeliveryV2Response
{
    public Models.Assets.Type assetTypeId { get; set; }
    public IEnumerable<AssetDeliveryEntry>? locations { get; set; }
}

public class ProductInfoWithAssetDelivery : ProductDataResponse
{
    public string? location { get; set; }
}

public class UsersResponseV1
{
    public string? description { get; set; }
    public string? created { get; set; }
}

public class AssetTypeEntry
{
    public int id { get; set; }
    public string name { get; set; }
}

public class AvatarAsset
{
    public long id { get; set; }
    public AssetTypeEntry assetType { get; set; }
}

public class AvatarResponse
{
    public IEnumerable<AvatarAsset> assets { get; set; }
}

public class MultiGetDetailsRequestEntry
{
    public string itemType { get; set; }
    public long id { get; set; }
}

public class MultiGetDetailsRequest
{
    public IEnumerable<MultiGetDetailsRequestEntry> items { get; set; }
}

public class MultiGetDetailsResponseEntry
{
    public long id { get; set; }
    public string itemType { get; set; }
    public int price { get; set; }
    public string creatorType { get; set; }
    public long creatorTargetId { get; set; }
}

public class MultiGetDetailsResponse
{
    public IEnumerable<MultiGetDetailsResponseEntry> data { get; set; }
}

public class CountResponse
{
    public long count { get; set; }
}

public class InventoryEntry
{
    public long assetId { get; set; }
}

public class InventoryResponse
{
    public IEnumerable<InventoryEntry> data { get; set; }
}

public class InvalidUserException : Exception
{
    public InvalidUserException() {}
}

public class BadgeEntry
{
    public int id { get; set; }
}

public class ProfileHeaderResponse
{
    [JsonPropertyName("IsVieweePremiumOnlyUser")]
    public bool isPremium { get; set; }
    [JsonPropertyName("ProfileDisplayName")]
    public string displayName { get; set; }
    [JsonPropertyName("PreviousUserNames")]
    public string previousUsernames { get; set; }
}

public class BundleItemEntry
{
    public long id { get; set; }
    public string name { get; set; }
    public string type { get; set; }
}

public class BundleProduct
{
    public long? priceInRobux { get; set; }
}

public class BundleResponseEntry
{
    public long id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string bundleType { get; set; }
    public List<BundleItemEntry> items { get; set; }
    public BundleProduct product { get; set; }
}

public class MultiGetBundlesResponse
{
    public List<BundleResponseEntry> data { get; set; }
}

public class RobloxApi
{
    private static HttpClient _client { get; } = new(new HttpClientHandler()
    {
        AutomaticDecompression = DecompressionMethods.All,
    });

    public async Task<ProductInfoWithAssetDelivery> GetProductInfoAssetDelivery(long assetId)
    {
        // Literally all it gets is the "assetTypeId". Everything else is blank.
        using var cancel = new CancellationTokenSource();
        cancel.CancelAfter(TimeSpan.FromSeconds(30));
        var response = await _client.GetAsync("https://assetdelivery.roblox.com/v2/asset?id=" + assetId, cancel.Token);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Unexpected status code from AssetDeliveryV2: " + response.StatusCode);
        }

        var str = await response.Content.ReadAsStringAsync(cancel.Token);
        var json = JsonSerializer.Deserialize<AssetDeliveryV2Response>(str);
        if (json == null)
            throw new Exception("Bad json from assetdelivery");
        return new ProductInfoWithAssetDelivery()
        {
            AssetTypeId = json.assetTypeId,
            Name = "Asset" + assetId,
            Description = "ConversionV1.0",
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow,
            
            location = json.locations?.FirstOrDefault(a => a.assetFormat == "source")?.location,
        };
    }

    private async Task<ProductDataResponse> GetProductInfoFromHtml(long assetId)
    {
        // why? well one reason: rate limits.
        // roblox heavily rate limits productinfo for some reason, but html doesn't seem as bad
        var watch = new Stopwatch();
        watch.Start();
        const int maxAttemptsMs = 60000;
        while (watch.ElapsedMilliseconds < maxAttemptsMs)
        {
            using var cancel = new CancellationTokenSource();
            cancel.CancelAfter(TimeSpan.FromMilliseconds(maxAttemptsMs));
            var url = $"https://www.roblox.com/catalog/{assetId}/-";
            var response = await _client.GetAsync(url, cancel.Token);
            if (!response.IsSuccessStatusCode)
            {
                Writer.Info(LogGroup.RealRobloxApi, "GetProductInfoHtml failed - " + response.StatusCode);
                await Task.Delay(TimeSpan.FromSeconds(2));
                continue;
            }

            var str = await response.Content.ReadAsStringAsync(cancel.Token);
            // regex
            var itemName = System.Web.HttpUtility.HtmlDecode(new Regex("data-item-name=\"(.+)\"").Match(str).Groups[1].Value);
            var assetTypeId = Enum.Parse<Models.Assets.Type>(new Regex("data-asset-type-id=\"(.+)\"").Match(str).Groups[1].Value);
            if (!Enum.IsDefined(assetTypeId))
            {
                throw new Exception("Invalid assetTypeId: " + assetTypeId);
            }
            // no way to get some values like created :(
            // we can technically get the description with regex but that might break easily
            return new ProductDataResponse()
            {
                Created = DateTime.UtcNow,
                AssetTypeId = assetTypeId,
                Description = "",
                Name = itemName,
                Updated = DateTime.UtcNow,
            };

        }

        throw new Exception("Timeout getting details from html");
    }

    public async Task<UsersResponseV1> GetUserInfo(long userId)
    {
        using var cancel = new CancellationTokenSource();
        cancel.CancelAfter(TimeSpan.FromSeconds(5));

        var result = await _client.GetAsync("https://users.roblox.com/v1/users/" + userId, cancel.Token);
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception("Unexpected response from Roblox: " + result.StatusCode);
        }

        var str = await result.Content.ReadAsStringAsync(cancel.Token);
        var json = JsonSerializer.Deserialize<UsersResponseV1>(str);
        if (json == null)
            throw new Exception("Null json returned from users api");
        return json;
    }

    public async Task<bool> DoesUserOwnAsset(long userId, long assetId)
    {
        using var cancel = new CancellationTokenSource();
        cancel.CancelAfter(TimeSpan.FromSeconds(5));

        var result =
            await _client.GetAsync(
                "https://inventory.roblox.com/v1/users/" + userId + "/items/Asset/" + assetId + "/is-owned",
                cancel.Token);
        if (!result.IsSuccessStatusCode)
            throw new Exception("Unexpected response: " + result.StatusCode);
        
        var str = await result.Content.ReadAsStringAsync(cancel.Token);
        return str switch
        {
            "true" => true,
            "false" => false,
            _ => throw new Exception("Unexpected response body: " + str)
        };
    }
    
    public async Task<ProductDataResponse> GetProductInfo(long assetId, bool allFieldsRequired = false)
    {
        var watch = new Stopwatch();
        watch.Start();
        const int maxAttemptTimeMs = 5000;
        
        while (watch.ElapsedMilliseconds < maxAttemptTimeMs)
        {
            try
            {
                using var cancel = new CancellationTokenSource();
                cancel.CancelAfter(TimeSpan.FromMilliseconds(maxAttemptTimeMs));
                var url = $"https://economy.roblox.com/v2/assets/{assetId}/details";
                //var url = $"https://api.roblox.com/marketplace/productinfo?assetId={assetId}"; deprecated api
                var result = await _client.GetAsync(url, cancel.Token);
                if (result.StatusCode is HttpStatusCode.TooManyRequests)
                {
                    Writer.Info(LogGroup.RealRobloxApi, "conversion error - got 429 during getproductinfo");
                    if (allFieldsRequired)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), cancel.Token);
                        continue;
                    }
                    break; // switch to html
                }
                if (!result.IsSuccessStatusCode)
                    throw new Exception("Unexpected response from Roblox: " + result.StatusCode + " (URL=" + url + ")");
                var str = await result.Content.ReadAsStringAsync(cancel.Token);
                var des = JsonSerializer.Deserialize<ProductDataResponse>(str);
                if (des == null)
                    throw new Exception("Null product data response from Roblox");
                return des;
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
        // last attempt
        return await GetProductInfoFromHtml(assetId);
    }

    public async Task<Stream> GetStreamAsync(string url)
    {
        var strResult = await _client.GetAsync(url);
        if (!strResult.IsSuccessStatusCode)
            throw new Exception("Bad response in GetStreamAsync: " + strResult.StatusCode);
        return await strResult.Content.ReadAsStreamAsync();
    }

    public async Task<Stream> GetAssetContent(long assetId)
    {
        while (true)
        {
            var result = await _client.GetAsync($"https://assetdelivery.roblox.com/v1/assetId/{assetId}");
            if (result.StatusCode is HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                continue;
            }
            if (!result.IsSuccessStatusCode)
                throw new Exception("Unexpected response from Roblox: " + result.StatusCode);
            var str = await result.Content.ReadAsStringAsync();
            var bod = JsonSerializer.Deserialize<AssetDeliveryResponse>(str);
            if (bod == null)
                throw new Exception("Null " + nameof(AssetDeliveryResponse) + " from Roblox");
            if (string.IsNullOrEmpty(bod.location))
                throw new Exception("Roblox did not give a URL for this asset content. Is the URL valid?");

            var strResult = await _client.GetAsync(bod.location);
            return await strResult.Content.ReadAsStreamAsync();
        }
    }

    private static Regex assetMatchUrlRegex = new Regex("data-mediathumb-url=\"(.+?)\"");
    
    public async Task<Stream> GetAssetAudioContent(long assetId)
    {
        var result = await _client.GetAsync($"https://www.roblox.com/library/{assetId}/--");
        if (!result.IsSuccessStatusCode)
            throw new Exception("Asset error: " + result.StatusCode);
        var bod = await result.Content.ReadAsStringAsync();
        var match = assetMatchUrlRegex.Match(bod);
        if (!match.Success)
            throw new Exception("Audio URL match failed for assetid = " + assetId);
        var groups = match.Groups.Values.ToArray();
        if (groups.Length < 1)
            throw new Exception("No match groups for audio URL");

        var fileUrl = groups[1].Value;

        var strResult = await _client.GetAsync(fileUrl);
        return await strResult.Content.ReadAsStreamAsync();
    }

    public async Task<AvatarResponse> GetAvatar(long userId)
    {
        var result = await _client.GetAsync("https://avatar.roblox.com/v1/users/" + userId + "/avatar");
        if (!result.IsSuccessStatusCode)
            throw new Exception("Avatar error: " + result.StatusCode);
        var body = await result.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<AvatarResponse>(body);
        if (json == null)
            throw new Exception("Null avatar response from Roblox");
        return json;
    }

    private string _csrf { get; set; } = "";
    
    public async Task<MultiGetDetailsResponse> MultiGetAssetDetails(IEnumerable<MultiGetDetailsRequestEntry> request)
    {
        var attempts = 0;
        var s = JsonSerializer.Serialize(new MultiGetDetailsRequest()
        {
            items = request,
        });
        while (true)
        {
            var msg = new HttpRequestMessage(HttpMethod.Post, "https://catalog.roblox.com/v1/catalog/items/details");
            msg.Content = new StringContent(s, Encoding.UTF8, "application/json");
            msg.Headers.Add("x-csrf-token", _csrf);
            
            var result = await _client.SendAsync(msg);
            if (result.StatusCode == HttpStatusCode.Forbidden && result.Headers.Contains("x-csrf-token"))
            {
                Writer.Info(LogGroup.RealRobloxApi, "use new csrf {0}", result.Headers.GetValues("x-csrf-token"));
                _csrf = result.Headers.GetValues("x-csrf-token").First();
                if (attempts > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(attempts));
                }
                attempts++;
                continue;
            }
            var body = await result.Content.ReadAsStringAsync();

            if (!result.IsSuccessStatusCode)
                throw new Exception("Get asset details request error: " + result.StatusCode + "\n" + body);
            var json = JsonSerializer.Deserialize<MultiGetDetailsResponse>(body);
            if (json == null)
                throw new Exception("Null multi-get response from Roblox");
            return json;
        }
    }

    public async Task<long> CountFollowers(long userId)
    {
        var result = await _client.GetAsync("https://friends.roblox.com/v1/users/"+userId+"/followers/count");
        if (!result.IsSuccessStatusCode)
            throw new Exception("Follower count error: " + result.StatusCode);
        var body = await result.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<CountResponse>(body);
        if (json == null)
            throw new Exception("Null follower count response from Roblox");
        return json.count;
    }
    
    public async Task<long> CountFriends(long userId)
    {
        var result = await _client.GetAsync("https://friends.roblox.com/v1/users/"+userId+"/friends/count");
        if (!result.IsSuccessStatusCode)
            throw new Exception("Friends count error: " + result.StatusCode);
        var body = await result.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<CountResponse>(body);
        if (json == null)
            throw new Exception("Null friends count response from Roblox");
        return json.count;
    }

    public async Task<InventoryResponse> GetInventory(long userId, string? cursor = null)
    {
        var url = "https://inventory.roblox.com/v2/users/"+userId+"/inventory?assetTypes=Hat%2CGear%2CHairAccessory%2CNeckAccessory%2CShoulderAccessory%2CBackAccessory%2CFrontAccessory%2CWaistAccessory&limit=100&sortOrder=Asc&cursor=" + (cursor ?? "");
        var result = await _client.GetAsync(url);
        if (result.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Forbidden)
            throw new InvalidUserException();
        
        if (!result.IsSuccessStatusCode)
            throw new Exception("Inventory error: " + result.StatusCode);
        var body = await result.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<InventoryResponse>(body);
        if (json == null)
            throw new Exception("Null inventory response from Roblox");
        return json;
    }

    public async Task<IEnumerable<BadgeEntry>> GetRobloxBadges(long userId)
    {
            var result = await _client.GetAsync("https://accountinformation.roblox.com/v1/users/"+userId+"/roblox-badges");
        if (!result.IsSuccessStatusCode)
            throw new Exception("Badges error: " + result.StatusCode);
        var body = await result.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<IEnumerable<BadgeEntry>>(body);
        if (json == null)
            throw new Exception("Null badges response from Roblox");
        return json;
    }

    public async Task<ProfileHeaderResponse> GetProfile(long userId)
    {
        var url = "https://www.roblox.com/users/profile/profileheader-json?userid=" + userId;
        var result = await _client.GetAsync(url);
        if (!result.IsSuccessStatusCode)
            throw new Exception("Premium error: " + result.StatusCode);
        var body = await result.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<ProfileHeaderResponse>(body);
        if (json == null)
            throw new Exception("Null profile response from Roblox");
        return json;
    }

    public async Task<BundleResponseEntry> GetBundle(long bundleId)
    {
        var url = "https://catalog.roblox.com/v1/bundles/details?bundleIds=" + bundleId; // MultiGetBundlesResponse
        var result = await _client.GetAsync(url);
        if (!result.IsSuccessStatusCode)
            throw new Exception("GetBundle error: " + bundleId + " " + result.StatusCode);
        var json = JsonSerializer.Deserialize<List<BundleResponseEntry>>(await result.Content.ReadAsStringAsync());
        if (json == null)
            throw new Exception("Null bundle response");
        if (json.Count != 1)
            throw new Exception("No bundle matches this id");
        return json[0];
    }

}