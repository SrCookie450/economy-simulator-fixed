using System.Collections.Generic;
using Xunit;

namespace Roblox.Libraries.UnitTest;

public class UnitTestUrlUtilities
{
    [Theory]
    [InlineData("www.roblox.com/asset/?id=2", 2)]
    [InlineData("https://web.sitetest2.robloxlabs.com/asset/?id=416&assetVersionId=695&skipScriptSigning=true", 416)]
    [InlineData("www.roblox.com/asset/?id=5000", 5000)]
    [InlineData("rbxassetid://69420", 69420)]
    [InlineData("6246271", 6246271)]
    [InlineData("https://www.roblox.com/catalog/6217671/hello-item", 6217671)]
    [InlineData("https://www.roblox.com/library/83893/hello-item", 83893)]
    public void Get_AssetId_From_Url(string url, long expectedAssetId)
    {
        var result = Roblox.Libraries.Assets.UrlUtilities.GetAssetIdFromUrl(url);
        Assert.Equal(expectedAssetId, result);
    }
}