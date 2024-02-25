using System.Text.RegularExpressions;

namespace Roblox.Libraries.Assets;

public static class UrlUtilities
{
    private static Regex urlRegex = new Regex("(library|catalog)\\/([0-9]+)\\/");
    private static Regex assetIdRegex = new Regex("id=([0-9]+)");
    
    public static long GetAssetIdFromUrl(string url)
    {
        url = url.ToLower();
        // Url can have a lot of formats:
        // rbxassetid://123
        // http://www.roblox.com/asset/?id=123
        // http://roblox.com/asset/?id=19999424
        // https://www.roblox.com/catalog/19999424/Shaggy
        // https://www.roblox.com/library/19999424/Sidesweep-Hair
        var index = url.IndexOf("rbxassetid://");

        if (index != -1) return long.Parse(url.Substring(index + "rbxassetid://".Length));

        index = url.IndexOf("/library/");
        if (index == -1)
        {
            index = url.IndexOf("/catalog/");
        }

        if (index != -1)
        {
            var match = urlRegex.Match(url);
            if (match.Success)
            {
                var matchGroups = match.Groups.Values.ToArray();
                if (matchGroups.Length >= 2)
                    return long.Parse(matchGroups[2].Value);
            }
        }

        index = url.IndexOf("?id=");
        if (index != -1)
        {
            var match = assetIdRegex.Match(url);
            if (match.Success)
            {
                var matchGroups = match.Groups.Values.ToArray();
                if (matchGroups.Length >= 1)
                    return long.Parse(matchGroups[1].Value);
            }
        }
        // Finally, just assume it's a number - parse will throw if we're wrong
        return long.Parse(url);
    }

    private static Regex quoteRegex = new("'", RegexOptions.Compiled);
    private static Regex seoNameNumberRegex = new Regex("[^a-zA-Z0-9]+", RegexOptions.Compiled);
    private static Regex symbolsRegex = new("^-+|-+$", RegexOptions.Compiled);
    private static Regex dotnetRoutingRegex = new("^(COM\\d|LPT\\d|AUX|PRT|NUL|CON|BIN)$");
    

    public static string ConvertToSeoName(string? name)
    {
        if (string.IsNullOrEmpty(name)) name = "";
        name = quoteRegex.Replace(name, "");
        name = seoNameNumberRegex.Replace(name, "-");
        name = symbolsRegex.Replace(name, "");
        name = dotnetRoutingRegex.Replace(name, "");
        if (string.IsNullOrEmpty(name)) return "unnamed";
        return name;
    }
}