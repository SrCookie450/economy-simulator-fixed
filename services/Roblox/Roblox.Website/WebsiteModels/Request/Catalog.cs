using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Roblox.Models;
using Roblox.Models.Assets;
using Type = Roblox.Models.Assets.Type;

namespace Roblox.Website.WebsiteModels.Catalog;

public class MultiGetEntry
{
    [DefaultValue("Asset")]
    public string itemType { get; set; }
    public long id { get; set; }
}

public class MultiGetRequest
{
    [MinLength(1), MaxLength(100)]
    public IEnumerable<MultiGetEntry> items { get; set; }
}

public class UpdateAssetRequest
{
    public string? description { get; set; } = null;
    public IEnumerable<Genre> genres { get; set; }
    public string name { get; set; }
    public bool isCopyingAllowed { get; set; }
    public bool enableComments { get; set; }
}

public class MultiGetAssetDetailsRequest
{
    public IEnumerable<long> assetIds { get; set; }
}

public class UpdateAssetPriceRequest
{
    public int? priceInRobux { get; set; }
    public int? priceInTickets { get; set; }
}

public class AddToProfileCollectionsRequest
{
    public long assetId { get; set; }
    public bool addToProfile { get; set; }
}

public class AddCommentRequest
{
    public long assetId { get; set; }
    public string text { get; set; }
}

public class UploadAssetRequest
{
    public string name { get; set; }
    public Type assetType { get; set; }
    public long? groupId { get; set; } = null;
    public IFormFile file { get; set; }
}

public class UploadAssetVersionRequest
{
    public long assetId { get; set; }
    public IFormFile file { get; set; }
}