using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Roblox.Rendering.Test;

public class RenderingTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RenderingTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task RenderGameThumb1080p()
    {
        CommandHandler.Configure("ws://localhost:3189", "hello world of deving 1234");
        var assetResult = await CommandHandler.RequestAssetGame(139, 1920, 1080);
        _testOutputHelper.WriteLine("Got result. Len={0}",assetResult.Length);
    }
}