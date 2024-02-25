using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Roblox.Libraries.TwitterApi;

public class TwitterPublicMetrics
{
    [JsonPropertyName("followers_count")]
    public long followersCount { get; set; }
    [JsonPropertyName("following_count")]
    public long followingsCount { get; set; }
    [JsonPropertyName("tweet_count")]
    public long tweetCount { get; set; }
    [JsonPropertyName("listed_count")]
    public long listedCount { get; set; }
}

public class TwitterUserObject
{
    [JsonPropertyName("id")] 
    public string userId { get; set; } = string.Empty;
    [JsonPropertyName("description")]
    public string? description { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime createdAt { get; set; }
    [JsonPropertyName("public_metrics")]
    public TwitterPublicMetrics? publicMetrics { get; set; }
}

public class GenericTwitterResponse<T>
{
    public IEnumerable<T> data { get; set; }
}

public class TwitterRecordNotFoundException : Exception
{
    
}

public class TwitterApi
{
    private static string? authorization { get; set; }
    private static HttpClient client { get; } = new();

    public static void Configure(string? newAuth)
    {
        if (!string.IsNullOrWhiteSpace(authorization))
            throw new Exception("Already configured");
        if (string.IsNullOrWhiteSpace(newAuth))
            throw new Exception("Bad token");
        authorization = newAuth;
    }

    public async Task<TwitterUserObject> GetUserByScreenName(string screenName)
    {
        var encodedName = System.Web.HttpUtility.UrlEncode(screenName);
        var request = new HttpRequestMessage(HttpMethod.Get,
            "https://api.twitter.com/2/users/by?usernames="+encodedName+"&user.fields=created_at,public_metrics,description&tweet.fields=author_id,created_at");
        request.Headers.Add("authorization", "Bearer "+ authorization);
        var result = await client.SendAsync(request);
        var str = await result.Content.ReadAsStringAsync();
        if (!result.IsSuccessStatusCode)
            throw new Exception("Twitter API Failure: " + result.StatusCode + " " + str);
        var json = JsonSerializer.Deserialize<GenericTwitterResponse<TwitterUserObject>>(str);
        if (json == null)
            throw new Exception("Null response from twitter");
        var user = json.data.FirstOrDefault();
        if (user == null)
            throw new TwitterRecordNotFoundException();
        return user;
    }
}