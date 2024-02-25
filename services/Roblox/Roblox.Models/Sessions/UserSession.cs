using System.Text.Json.Serialization;
using Roblox.Models.Users;

namespace Roblox.Models.Sessions
{
    public class UserSession
    {
        public string sessionId { get; set; }
        public long userId { get; set; }
        public string username { get; set; }
        public DateTime created { get; set; }
        public Users.AccountStatus accountStatus { get; set; }
        public int sessionKey { get; set; }
        public bool isImpersonating { get; set; }
        
        [Obsolete("Unused - not sure why this was added")]
        public bool isGame { get; set; }
        // Legacy permission. Please do not use in new work.

        public UserSession(long userId, string username, DateTime created,
            Users.AccountStatus accountStatus, int sessionKey, bool isImpersonating, string sessionId)
        {
            this.userId = userId;
            this.username = username;
            this.created = created;
            this.accountStatus = accountStatus;
            this.sessionKey = sessionKey;
            this.isImpersonating = isImpersonating;
            this.sessionId = sessionId;
        }
    }

    public class UserAgentBypass
    {
        [JsonPropertyName("a")]
        public string version { get; set; } = "1";
        [JsonPropertyName("b")]
        public string ipAddress { get; set; }
        [JsonPropertyName("c")]
        public string userAgent { get; set; }
        [JsonPropertyName("d")]
        public DateTime createdAt { get; set; }

        public string GetSalt()
        {
            return createdAt.ToString() + "," + userAgent;
        }
    }
}

