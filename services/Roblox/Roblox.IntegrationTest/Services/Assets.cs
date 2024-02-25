using System.Threading.Tasks;
using Xunit;

namespace Roblox.Services.IntegrationTest;

public class AssetsServiceIntegrationTest : TestBase
{
    [Fact]
    public async Task PermissionsCheck_User()
    {
        var assets = ServiceProvider.GetOrCreate<AssetsService>();
        var userOne = await CreateRandomUser();
        var userTwo = await CreateRandomUser();
        var asset = await CreateRandomItem(userOne);

        // Create must be able to modify their own item
        var canUserOneModify = await assets.CanUserModifyItem(asset, userOne);
        Assert.True(canUserOneModify);
        
        // Non creator user cannot modify this item
        var canUserTwoModify = await assets.CanUserModifyItem(asset, userTwo);
        Assert.False(canUserTwoModify);
    }
    
    [Fact]
    public async Task PermissionsCheck_Group()
    {
        var assets = ServiceProvider.GetOrCreate<AssetsService>();
        var userOne = await CreateRandomUser();
        var userTwo = await CreateRandomUser();
        var groupOne = await CreateRandomGroup(userOne);
        var asset = await CreateRandomGroupItem(groupOne);

        // Create must be able to modify their own item
        var canUserOneModify = await assets.CanUserModifyItem(asset, userOne);
        Assert.True(canUserOneModify);
        
        // Non creator user cannot modify this item
        var canUserTwoModify = await assets.CanUserModifyItem(asset, userTwo);
        Assert.False(canUserTwoModify);
    }
}