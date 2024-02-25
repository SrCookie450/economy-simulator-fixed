using System.Text.RegularExpressions;
using Roblox.Dto.Avatar;
using Roblox.Dto.Users;
using Roblox.Models.Assets;
using Roblox.Models.Avatar;
using Roblox.Models.Users;
using Roblox.Rendering;
using Roblox.Services;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;
using Roblox.Website.WebsiteModels.Asset;
using BadRequestException = Roblox.Exceptions.BadRequestException;
using ServiceProvider = Roblox.Services.ServiceProvider;
using Type = Roblox.Models.Assets.Type;

namespace Roblox.Website.Controllers
{
    /// <summary>
    /// This class extends ControllerServices. It is meant for extension methods that are used by multiple controllers, but also require access to multiple services.
    /// </summary>
    public class ControllerServicesExtended : ControllerServices
    {

        [Obsolete("Use AvatarService.RedrawAvatar instead")]
        public static async Task RedrawAvatar(long userId, IEnumerable<long>? newAssetIds = null, ColorEntry? colors = null,
            AvatarType? avatarType = null, bool forceRedraw = false, bool ignoreLock = false)
        {
            using var avatar = ServiceProvider.GetOrCreate<AvatarService>();
            await avatar.RedrawAvatar(userId, newAssetIds, colors, avatarType, forceRedraw, ignoreLock);
        }
        
        [Obsolete("Use MigrateItem.MigrateItemFromRoblox() instead")]
        public async Task<MigrateItem> MigrateItemFromRoblox(string robloxUrl, bool isForSale = false, int? price = null, IEnumerable<Models.Assets.Type>? allowedTypes = null)
        {
            return await MigrateItem.MigrateItemFromRoblox(robloxUrl, isForSale, price, allowedTypes);
        }
        
        [Obsolete("Use InventoryService instead")]
        public async Task<bool> CanViewInventory(long userId, long contextUserId = 0)
        {
            using var inv = ServiceProvider.GetOrCreate<InventoryService>();
            return await inv.CanViewInventory(userId, contextUserId);
        }

        [Obsolete("Use InventoryService instead")]
        public async Task<IEnumerable<Roblox.Dto.Users.CanViewInventoryEntry>> MultiCanViewInventory(IEnumerable<long> userIds, long contextUserId = 0)
        {
            using var inv = ServiceProvider.GetOrCreate<InventoryService>();
            return await inv.MultiCanViewInventory(userIds, contextUserId);
        }
    }
}

