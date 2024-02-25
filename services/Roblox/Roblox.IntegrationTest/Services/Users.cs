using System.Threading.Tasks;
using Roblox.Models.Users;
using Xunit;

namespace Roblox.Services.IntegrationTest;

public class UnitTest1 : TestBase
{
    [Fact]
    public async Task SignUp_Login_and_GetUser()
    {
        var userService = new UsersService();
        // Sign up
        var user = await userService.CreateUser("AmogusDrip", "password123", Gender.Male, null);
        // Confirm password was hashed correctly
        var loginOk = await userService.VerifyPassword(user.userId, "password123");
        Assert.True(loginOk);
        // Confirm random password isn't ok
        var shouldBeFalse = await userService.VerifyPassword(user.userId, "password123FAKE");
        Assert.False(shouldBeFalse);
        // Get info
        var info = await userService.GetUserById(user.userId);
        Assert.NotNull(info);
        Assert.True(info.userId == user.userId);
        Assert.Equal("AmogusDrip", info.username);
    }
    

}