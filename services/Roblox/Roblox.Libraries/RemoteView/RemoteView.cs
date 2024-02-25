using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Roblox.Libraries.RemoteView;

public static class RemoteView
{
    private static HttpClient client { get; } = new();
    private static string baseUrl { get; set; } = string.Empty;
    private static string authorization { get; set; } = string.Empty;

    public static void Configure(string newBaseUrl, string newAuthorization)
    {
        authorization = newAuthorization;
        baseUrl = newBaseUrl;
    }

    public static async Task<string> GetView(string viewName, IEnumerable<dynamic> arguments)
    {
        var serialized = JsonSerializer.Serialize(arguments);
        var c = new StringContent(serialized);
        c.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        c.Headers.Add("rblx-authorization", authorization);
        var result = await client.PostAsync(baseUrl + "/api/get-view?view=" + viewName, c);
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception("Request failed with statusCode=" + result.StatusCode);
        }

        return await result.Content.ReadAsStringAsync();
    }
}