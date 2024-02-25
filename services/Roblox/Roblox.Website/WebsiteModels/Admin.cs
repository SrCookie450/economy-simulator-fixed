// ReSharper disable InconsistentNaming

using Roblox.Models.Economy;

namespace Roblox.Dto.Admin;
using Roblox.Models.Assets;

public class SetAlertRequest
{
    public string? text { get; set; }
    public string? url { get; set; }
}

public class CreateUserRequest
{
    public string? username { get; set; }
    public string? password { get; set; }
    public long userId { get; set; }
}

public class ForceApplicationReq
{
    public long userId { get; set; }
    public string? socialURL { get; set; }
}

public class PendingAssetEntry
{
    public long id { get; set; }
    public string name { get; set; } = string.Empty;
    public string? content_url { get; set; }
    public long creatorId { get; set; }
    public string creatorName { get; set; } = string.Empty;
    public Type assetType { get; set; }
}

public class ModerateAssetRequest
{
    public long assetId { get; set; }
    public bool isApproved { get; set; }
    public bool is18Plus { get; set; }
}

public class AssetModerationStatus
{
    public long? robloxAssetId { get; set; }
    public ModerationStatus moderationStatus { get; set; }

    public bool canEarnRobuxFromApproval =>
        moderationStatus is ModerationStatus.AwaitingApproval or ModerationStatus.AwaitingModerationDecision;
}

public class ModerateIconRequest
{
    public long iconId { get; set; }
    public bool isApproved { get; set; }
    public bool is18Plus { get; set; }
}

public class IconToggleRequest
{
    public string? name { get; set; }
    public long groupId { get; set; }
    public int approved { get; set; }
}

public class BanUserRequest
{
    public long userId { get; set; }
    public string reason { get; set; } = string.Empty;
    public string? internalReason { get; set; }
    public string? expires { get; set; }
}

public class CreateMessageRequest
{
    public long userId { get; set; }
    public string subject { get; set; } = string.Empty;
    public string body { get; set; } = string.Empty;
}

public class UserIdRequest
{
    public long userId { get; set; }
}

public class GiveBadgeRequest
{
    public long badgeId { get; set; }
    public long userId { get; set; }
}

public class GiveUserTicketsRequest
{
    public long userId { get; set; }
    public long tickets { get; set; }
}


public class GiveUserRobuxRequest
{
    public long userId { get; set; }
    public long robux { get; set; }
}

public class RemoveItemRequest
{
    public long userId { get; set; }
    public long userAssetId { get; set; }
}

public class GiveItemRequest
{
    public long userId { get; set; }
    public long assetId { get; set; }
    public int copies { get; set; } = 1;
    public bool giveSerial { get; set; } = false;
}

public class DeleteUsernameRequest
{
    public string username { get; set; } = string.Empty;
    public long userId { get; set; }
}

public class DeleteForumPostRequest
{
    public long postId { get; set; }
}

public class ReRenderRequest
{
    public long assetId { get; set; }
}

public class UpdateProductRequest
{
    public long assetId { get; set; }
    public bool isForSale { get; set; }
    public bool isLimited { get; set; }
    public bool isLimitedUnique { get; set; }
    public int? priceRobux { get; set; }
    public int? priceTickets { get; set; }
    public int? maxCopies { get; set; }
    public DateTime? offsaleDeadline { get; set; }
}

public class UpdateNameRequest
{
    // TODO: Add Description Support.
    public long assetId { get; set; }
    public string newName { get; set; }
}


public class CreateAssetRequest
{
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public Type assetTypeId { get; set; }
    public Genre genre { get; set; }
    public bool isForSale { get; set; }
    public bool isLimited { get; set; }
    public bool isLimitedUnique { get; set; }
    public int? price { get; set; }
    public int? maxCopies { get; set; }
    public DateTime? offsaleDeadline { get; set; }
    public long? robloxAssetId { get; set; }
    public IFormFile? rbxm { get; set; }
    public string? packageAssetIds { get; set; }
}

public class CreateClothingRequest
{
    public string name { get; set; } = string.Empty;
    public string? description { get; set; }
    public Type assetTypeId { get; set; }
    public Genre genre { get; set; }
    public bool isForSale { get; set; }
    public int? price { get; set; }
    public long? robloxAssetId { get; set; }
    public IFormFile? file { get; set; }
}

public class MigrateItemRequest
{
    public long assetId { get; set; }
    public bool isForSale { get; set; }
    public int? price { get; set; }
}

public class MigrateItemAlternateRequest
{
    public string url { get; set; } = string.Empty;
    public bool disableRender { get; set; }
}

public class CreateAssetVersionRequest
{
    public long assetId { get; set; }
    public IFormFile? rbxm { get; set; }
}

public class DateTimeSerialized
{
    public DateTime clock { get; set; }
}

public class CopyAssetRequest
{
    public long assetId { get; set; }
    public bool force { get; set; }
}

public class AssetVersionWithIdEntry
{
    public long assetId { get; set; }
}

public class RefundTransactionEntry
{
    public long id { get; set; }
    public CurrencyType currencyType { get; set; }
    public long amount { get; set; }
    public long? userAssetId { get; set; }
    public long userId { get; set; }
    public long otherUserId { get; set; }
    public long assetId { get; set; }
}