using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Roblox.Models.Sessions;
using Roblox.Services;
using Roblox.Services.Exceptions;

namespace Roblox.Website.Controllers
{
    public class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        protected ControllerServices services { get; set; }
        
#if DEBUG
        public UserSession? userSessionForTests { get; set; }
#endif

        protected Roblox.Models.Sessions.UserSession? userSession
        {
            get
            {
#if DEBUG
                if (userSessionForTests != null)
                    return userSessionForTests;
#endif
                var dict = HttpContext.Items;
                if (dict.ContainsKey(Roblox.Website.Middleware.SessionMiddleware.CookieName))
                {
                    return (UserSession?)dict[Middleware.SessionMiddleware.CookieName];
                }

                return null;
            }
        }

        /// <summary>
        /// Equivalent to userSession but it will throw "Unauthorized" if user is not logged in.
        /// </summary>
        /// <exception cref="RobloxException"></exception>
        protected Roblox.Models.Sessions.UserSession safeUserSession
        {
            get
            {
                if (userSession == null)
                    throw new RobloxException(401, 0, "Unauthorized");
                return userSession;
            }
        }

        public ControllerBase()
        {
            services = new ControllerServicesExtended();
        }

        public static string GetRequesterIpRaw(HttpContext ctx)
        {
            Debug.Assert(ctx != null);
            // Check for cloudflare
            var headers = ctx.Request.Headers;
            if (headers.ContainsKey("cf-connecting-ip"))
            {
                return headers["cf-connecting-ip"];
            }

            var ipString = ctx.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipString))
                throw new Exception("Bad IP address - empty or null string");
            return ipString;
        }

        private string GetRequesterIpRaw()
        {
            return GetRequesterIpRaw(HttpContext);
        }

        private static Dictionary<string, string> ipToKeyMap { get; } = new();
        private static Mutex ipToKeyMux { get; } = new();

        private class RedisIpHashSetupV1
        {
            public Dictionary<string,string> digitToGuid { get; set; }
            public string endKey { get; set; }
        }
        
        private static RedisIpHashSetupV1? redisIpHashSetup { get; set; }
        private static Mutex redisIpHashSetupMux = new();
        public static void InitializeIpHashSetup()
        {
            if (redisIpHashSetup != null)
                return;
            
            lock (redisIpHashSetupMux)
            {
                if (redisIpHashSetup != null)
                    return;

                var key = "IpHashKeyV1";
                var data = Roblox.Services.Cache.distributed.StringGet(key);
                if (data == null)
                {
                    var c = new RedisIpHashSetupV1
                    {
                        digitToGuid = new Dictionary<string, string>()
                    };
                    for (var i = 0; i < 10; i++)
                    {
                        c.digitToGuid[i.ToString()] = Guid.NewGuid().ToString() + Guid.NewGuid();
                    }

                    c.endKey = Guid.NewGuid().ToString() + Guid.NewGuid();
                    Roblox.Services.Cache.distributed.StringSet(key, JsonSerializer.Serialize(c));
                    redisIpHashSetup = c;
                }
                else
                {
                    redisIpHashSetup = JsonSerializer.Deserialize<RedisIpHashSetupV1>(data);
                }
            }
        }
        
        public static ulong ConvertFromIpAddressToInteger(string ipAddress)
        {
            var address = IPAddress.Parse(ipAddress);
            address = address.MapToIPv6();
            byte[] bytes = address.GetAddressBytes();

            // flip big-endian(network order) to little-endian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            
            return BitConverter.ToUInt64(bytes, 0);
        }
        
        /// <summary>
        /// GetIP returns a hashed version of the current user's IP address.
        /// </summary>
        /// <remarks>
        /// The specific hash format should not matter, in case we need to update this some day.
        /// </remarks>
        /// <returns>A hash of an arbitrary size</returns>
        public static string GetIP(string trueIp, string? salt = null)
        {
            InitializeIpHashSetup();
            // Convert IP to a unit
            var ip = ConvertFromIpAddressToInteger(trueIp);
            // Get first digit in ip
            var first = ip.ToString().Substring(0, 1);
            // Get last digit in ip
            var last = ip.ToString().Substring(ip.ToString().Length - 1);
            var keyToUse = redisIpHashSetup.digitToGuid[first];
            // randomize
            if (first is "2" or "6" or "3" or "7")
                keyToUse = new string(keyToUse.ToCharArray().Reverse().ToArray());
            
            var key = keyToUse + ip;
            if (last != "9")
                key += redisIpHashSetup.digitToGuid[last];
            else
                key += redisIpHashSetup.digitToGuid[last].ToUpper();
            
            for (var i = 0; i < ip.ToString().Length; i++)
            {
                var toAdd = redisIpHashSetup.digitToGuid[ip.ToString()[i].ToString()];
                if (toAdd.Length >= i)
                {
                    toAdd = toAdd.Substring(i, i+1);
                }
                else
                {
                    toAdd = toAdd.Substring(0, 2);
                }
                key += toAdd;
            }

            if (salt != null)
                key += salt;

            using var alg = SHA512.Create();
            var hash = alg.ComputeHash(Encoding.UTF8.GetBytes(key + redisIpHashSetup.endKey));
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Get the request's IP hash
        /// </summary>
        /// <returns></returns>
        protected string GetIP(string? salt = null)
        {
            return GetIP(GetRequesterIpRaw(), salt);
        }
    }
}

