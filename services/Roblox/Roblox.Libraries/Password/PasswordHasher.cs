using Sodium;

namespace Roblox.Libraries.Password
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            var hash = PasswordHash.ArgonHashString(password);
            if (hash == null) throw new Exception("PasswordHasher null hash");
            // seems to be a common problem with argon2 implementations... https://github.com/scottbrady91/ScottBrady91.AspNetCore.Identity.Argon2PasswordHasher/issues/2
            hash = hash.Replace("\0", "");
            return hash;
        }

        public bool Verify(string hash, string password)
        {
            return PasswordHash.ArgonHashStringVerify(hash, password);
        }
    }
}