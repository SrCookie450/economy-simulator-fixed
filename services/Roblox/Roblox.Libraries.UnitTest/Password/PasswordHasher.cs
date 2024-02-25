using Roblox.Libraries.Password;
using Xunit;

namespace Roblox.Libraries.UnitTest
{
    public class UnitTestPasswordHasher
    {
        [Fact]
        public void Password_Create_And_Verify_Hash()
        {
            var p = new PasswordHasher();
            var result = p.Hash("Hello World 123");
            Assert.NotNull(result);
            Assert.True(result.Length > 10);
            var ok = p.Verify(result, "Hello World 123");
            // Confirm password is OK
            Assert.True(ok);
            var notOk = p.Verify(result, "Hello World 1234");
            // We gave the wrong password. Assert not true.
            Assert.False(notOk);
        }
        
        [Fact]
        public void Verify_NodeJS_Generated_Argon_Hash()
        {
            var nodeJsHash = "$argon2i$v=19$m=4096,t=3,p=1$XuaGTXj6aDCMXIWuA3/jiA$rOPVm7YQ2EyggUMgj+/V2nlG/FiFU4yGNHwxs/w08ig";
            var nodeJsPass = "NodeJS Password Test";
            var p = new PasswordHasher();
            var result = p.Verify(nodeJsHash, nodeJsPass);
            Assert.True(result);
        }
    }
}