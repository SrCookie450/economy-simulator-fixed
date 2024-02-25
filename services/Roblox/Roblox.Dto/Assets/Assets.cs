using System.Text.Json.Serialization;
using Roblox.Models.Assets;
using Type = Roblox.Models.Assets.Type;

namespace Roblox.Dto.Assets
{
    public class AssetId
    {
        public long assetId { get; set; }
    }

    public class AssetIdWithType
    {
        public long assetId { get; set; }
        public Models.Assets.Type assetType { get; set; }
    }

    public class CreateResponse
    {
        public long assetId { get; set; }
        public long assetVersionId { get; set; }
        public ModerationStatus moderationStatus { get; set; }
    }

    public class CreatePlaceResponse
    {
        public long placeId { get; set; }
    }

    public class ProductEntry
    {
        public string name { get; set; }
        public bool isForSale { get; set; }
        public bool isLimited { get; set; }
        public bool isLimitedUnique { get; set; }
        public int? priceRobux { get; set; }
        public int? priceTickets { get; set; }
        public int? serialCount { get; set; }
        public DateTime? offsaleAt { get; set; }
    }

    public class MultiGetEntryLowestSeller
    {
        public long userId { get; set; }
        public string username { get; set; }
        public long userAssetId { get; set; }
        public long price { get; set; }
        public long assetId { get; set; }
    }

    public class MultiGetEntryInternal
    {
        public long id { get; set; }
        public Models.Assets.Type assetType { get; set; }
        public string name { get; set; }
        public string? description { get; set; }
        
        public Genre genre { get; set; }
        
        public CreatorType creatorType { get; set; }
        public long creatorTargetId { get; set; }
        public DateTime? offsaleDeadline { get; set; }
        public bool isForSale { get; set; }
        public int? priceRobux { get; set; }
        public int? priceTickets { get; set; }
        public bool isLimited { get; set; }
        public bool isLimitedUnique { get; set; }
        public int serialCount { get; set; }
        public int saleCount { get; set; }
        public long favoriteCount { get; set; }
        public string groupName { get; set; }
        public string username { get; set; }
        public bool is18Plus { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public ModerationStatus moderationStatus { get; set; }
        public MultiGetEntryLowestSeller? lowestSellerData { get; set; }
    }

    public class MultiGetEntry
    {
        public MultiGetEntry()
        {
            
        }

        public MultiGetEntry(MultiGetEntryInternal internalEntry)
        {
            id = internalEntry.id;
            assetType = internalEntry.assetType;
            name = internalEntry.name;
            description = internalEntry.description;
            genres = new[] {internalEntry.genre};
            creatorType = internalEntry.creatorType;
            creatorTargetId = internalEntry.creatorTargetId;
            if (internalEntry.creatorType == CreatorType.Group)
            {
                creatorName = internalEntry.groupName;
            }
            else
            {
                creatorName = internalEntry.username;
            }
            
            offsaleDeadline = internalEntry.offsaleDeadline;
            is18Plus = internalEntry.is18Plus;
            moderationStatus = internalEntry.moderationStatus;
            var restrictions = new List<string>();
            saleCount = internalEntry.saleCount;
            favoriteCount = internalEntry.favoriteCount;
            isForSale = internalEntry.isForSale && (internalEntry.priceRobux != null || internalEntry.priceTickets != null);
            price = internalEntry.priceRobux;
            priceTickets = internalEntry.priceTickets;
            createdAt = internalEntry.createdAt;
            updatedAt = internalEntry.updatedAt;
            lowestSellerData = internalEntry.lowestSellerData;
            
            // Special stuff
            serialCount = internalEntry.serialCount;
            if (internalEntry.lowestSellerData != null)
            {
                lowestPrice = internalEntry.lowestSellerData.price;
            }
            if (internalEntry.isLimited && !internalEntry.isLimitedUnique)
            {
                restrictions.Add("Limited");
            }
            else if (internalEntry.isLimitedUnique)
            {
                restrictions.Add("LimitedUnique");
            }
            // unitsAvailableForConsumption can appear on both Limited AND LimitedU items.
            if (internalEntry.serialCount != 0)
            {
                var available = internalEntry.serialCount - internalEntry.saleCount;
                if (available <= 0)
                {
                    isForSale = false;
                }
                else
                {
                    unitsAvailableForConsumption = available;
                }
            }

            itemRestrictions = restrictions;
        }
        public long id { get; set; }
        [JsonConverter(typeof(JsonIntEnumConverter<Type>))]
        public Models.Assets.Type assetType { get; set; }
        public string name { get; set; }
        public string? description { get; set; }
        
        public IEnumerable<Genre> genres { get; set; }
        
        public CreatorType creatorType { get; set; }
        public long creatorTargetId { get; set; }
        public string creatorName { get; set; }
        public DateTime? offsaleDeadline { get; set; }
        public IEnumerable<string> itemRestrictions { get; set; }
        public int saleCount { get; set; }
        public string itemType { get; set; } = "Asset";
        public long? favoriteCount { get; set; } = null;
        public bool isForSale { get; set; }
        public long? price { get; set; }
        public long? priceTickets { get; set; }
        public long? lowestPrice { get; set; } = null;
        public string? priceStatus
        {
            get
            {
                if (isForSale && price == 0) return "Free";
                if (itemRestrictions != null &&
                    (itemRestrictions.Contains("Limited") || itemRestrictions.Contains("LimitedUnique")))
                {
                    if (lowestSellerData == null) return "No Resellers";
                }
                return null;
            }
        }
        
        public MultiGetEntryLowestSeller? lowestSellerData { get; set; }
        public int? unitsAvailableForConsumption { get; set; } = null;
        public int? serialCount { get; set; } = null;
        public bool is18Plus { get; set; }
        public ModerationStatus moderationStatus { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
    
    public class RecommendedItemEntry
    {
        public long assetId { get; set; }
        public string name { get; set; }
        public int? price { get; set; }
        public long creatorId { get; set; }
        public CreatorType creatorType { get; set; }
        public string creatorName { get; set; }
        public bool isForSale { get; set; }
        public bool isLimited { get; set; }
        public bool isLimitedUnique { get; set; }
        public DateTime? offsaleDeadline { get; set; }
    }

    public class MultiGetAssetDeveloperDetailsDb
    {
        public long assetId { get; set; }
        public int typeId { get; set; }
        public Genre genre { get; set; }
        public CreatorType creatorType { get; set; }
        public long creatorId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public bool enableComments { get; set; }
        public ModerationStatus moderationStatus { get; set; }
        public bool is18Plus { get; set; }
    }

    public class CreatorEntry
    {
        public CreatorType type { get; set; }
        public int typeId => (int) type;
        public long targetId { get; set; }
    }
    
    public class MultiGetAssetDeveloperDetails
    {
        public long assetId { get; set; }
        public int typeId { get; set; }
        public IEnumerable<Genre> genres { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public CreatorEntry creator { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public ModerationStatus moderationStatus { get; set; }
        public bool is18Plus { get; set; }
        public bool enableComments { get; set; }
        public bool isCopyingAllowed { get; set; }
        public bool isPublicDomainEnabled { get; set; }
        public bool isVersioningEnabled { get; set; }
        public bool isArchivable { get; set; }
        public bool canHaveThumbnail { get; set; }

        public MultiGetAssetDeveloperDetails(MultiGetAssetDeveloperDetailsDb dbResult)
        {
            assetId = dbResult.assetId;
            typeId = dbResult.typeId;
            genres = new Genre[] {dbResult.genre};
            name = dbResult.name;
            description = dbResult.description;
            creator = new CreatorEntry()
            {
                type = dbResult.creatorType,
                targetId = dbResult.creatorId,
            };
            created = dbResult.created;
            updated = dbResult.updated;
            enableComments = dbResult.enableComments;
            moderationStatus = dbResult.moderationStatus;
            is18Plus = dbResult.is18Plus;
        }
    }
    
    public class CreationEntry
    {
        public long assetId { get; set; }
        public string name { get; set; }
    }

    public class IsAsset18Plus
    {
        public bool is18Plus { get; set; }
    }

    public class StaffAssetCommentEntry
    {
        public long id { get; set; }
        public long assetId { get; set; }
        public string name { get; set; }
        public long userId { get; set; }
        public string username { get; set; }
        public string comment { get; set; }
        public DateTime createdAt { get; set; }
    }
}