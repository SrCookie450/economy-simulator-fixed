using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Roblox.Dto.Assets;
using Roblox.Models;
using Roblox.Models.Assets;
using Roblox.Services;
using Roblox.Website.WebsiteModels;
using Roblox.Website.WebsiteModels.Catalog;
using MultiGetEntry = Roblox.Dto.Assets.MultiGetEntry;
#pragma warning disable CS8600

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/catalog/v1")]
public class CatalogControllerV1 : ControllerBase
{
    [HttpGet("categories")]
    public dynamic GetCategories()
    {
        return new
        {
            Featured = 0,
            All = 1,
            Collectibles = 2,
            Clothing = 3,
            BodyParts = 4,
            Gear = 5,
            Models = 6,
            Plugins = 7,
            Decals = 8,
            Audio = 9,
            Meshes = 10,
            Accessories = 11,
            AvatarAnimations = 12,
            CommunityCreations = 13,
            Video = 14,
            Recommended = 15,
        };
    }

    [HttpGet("subcategories")]
    public dynamic GetSubcategories()
    {
        return new
        {
            Featured = 0,
            All = 1,
            Collectibls = 2,
            Clothing = 3,
            BodyParts = 4,
            Gear = 5,
            Models = 6,
            Plugins = 7,
            Decals = 8,
            Hats = 9,
            Faces = 10,
            Packages = 11,
            Shirts = 12,
            Tshirts = 13,
            Pants = 14,
            Heads = 15,
            Audio = 16,
            RobloxCreated = 17,
            Meshes = 18,
            Accessories = 19,
            HairAccessories = 20,
            FaceAccessories = 21,
            NeckAccessories = 22,
            ShoulderAccessories = 23,
            FrontAccessories = 24,
            BackAccessories = 25,
            WaistAccessories = 26,
            AvatarAnimations = 27,
            ClimbAnimations = 28,
            FallAnimations = 30,
            IdleAnimations = 31,
            JumpAnimations = 32,
            RunAnimations = 33,
            SwimAnimations = 34,
            WalkAnimations = 35,
            AnimationPackage = 36,
            Bundles = 37,
            AnimationBundles = 38,
            EmoteAnimations = 39,
            CommunityCreations = 40,
            Video = 41,
            Recommended = 51,
        };
    }

    [HttpGet("search/navigation-menu-items")]
    public dynamic GetSearchNavigationMenuItems()
    {
        return new
        {
            defaultGearSubcategory = 5,
            defaultCategory = 0,
            defaultCategoryIdForRecommendedSearch = 0,
            defaultCreator = 0,
            defaultCurrency = 0,
            defaultSortType = 0,
            defaultSortAggregation = 5,
            categoriesWithCreator = new List<int>() { 1, 3, 13},
            robloxUserId = 1,
            robloxUserName = "ROBLOX",
            gearSubcategory = 5,
            allCategories = 1,
            freeFilter = 5,
            customRobuxFilter = 3,
            robuxFilter = 1,
            categories = new List<dynamic>()
            {
                new
                {
                    category = "All",
                    categoryId = 1,
                    name = "All Categories",
                    orderIndex = 1,
                    subcategories = new List<dynamic>(),
                    isSearchable = true,
                },
                new
                {
                    category = "Featured",
                    categoryId = 0,
                    name = "Featured",
                    orderIndex = 2,
                    subcategories = new List<dynamic>()
                    {
                        new {
                            subcategory = "Featured",
                            subcategoryId = 0,
                            name = "All Featured Items",
                            shortName = "All",
                        },
                        new {
                            subcategory = "Accessories",
                            subcategoryId = 19,
                            name = "Featured Accessories",
                            shortName = "Accessories",
                        },
                        /*
                        {
                            subcategory = "AnimationBundles",
                            subcategoryId = 38,
                            name = "Featured Animations",
                            shortName = "Animations",
                        },
                        */
                        new {
                            subcategory = "Faces",
                            subcategoryId = 10,
                            name = "Featured Faces",
                            shortName = "Faces",
                        },
                        new {
                            subcategory = "Gear",
                            subcategoryId = 5,
                            name = "Featured Gear",
                            shortName = "Gear",
                        },
                        /*
                        {
                            subcategory = "Bundles",
                            subcategoryId = 37,
                            name = "Featured Bundles",
                            shortName = "Bundles",
                        },
                        {
                            subcategory = "EmoteAnimations",
                            subcategoryId = 39,
                            name = "Featured Emotes",
                            shortName = "Emotes",
                        },
                        */
                    },
                    isSearchable = true,
                },
                new
                {
                    category = "CommunityCreations",
                    categoryId = 13,
                    name = "Community Creations",
                    orderIndex = 3,
                    subCategories = new List<dynamic>()
                    {
                        new {
                            subcategory = "CommunityCreations",
                            subcategoryId = 40,
                            name = "All Creations",
                            shortName = (string)null,
                        },
                        new {
                            subcategory = "Hats",
                            subcategoryId = 9,
                            name = "Hats",
                            shortName = (string)null,
                        },
                        new {
                            subcategory = "HairAccessories",
                            subcategoryId = 20,
                            name = "Hair",
                            shortName = (string)null,
                        },
                        new {
                            subcategory = "FaceAccessories",
                            subcategoryId = 21,
                            name = "Face",
                            shortName = (string)null,
                        },
                        new {
                            subcategory = "NeckAccessories",
                            subcategoryId = 22,
                            name = "Neck",
                            shortName = (string)null,
                        },
                        new {
                            subcategory = "ShoulderAccessories",
                            subcategoryId = 23,
                            name = "Shoulder",
                            shortName = (string)null,
                        },
                        new {
                            subcategory = "FrontAccessories",
                            subcategoryId = 24,
                            name = "Front",
                            shortName = (string)null,
                        },
                        new {
                            subcategory = "BackAccessories",
                            subcategoryId = 25,
                            name = "Back",
                            shortName = (string)null,
                        },
                        new {
                            subcategory = "WaistAccessories",
                            subcategoryId = 26,
                            name = "Waist",
                            shortName = (string)null,
                        },
                    },
                    isSearchable = true,
                },
                new
                {
                    category = "Collectibles",
                    categoryId = 2,
                    name = "Collectibles",
                    orderIndex = 5,
                    subcategories = new List<dynamic>()
                    {
                        new {
                            subcategory = "Collectibles",
                            subcategoryId = 2,
                            name = "All Collectibles",
                            shortName = "All",
                        },
                        new {
                            subcategory = "Accessories",
                            subcategoryId = 19,
                            name = "Collectible Accessories",
                            shortName = "Accessories",
                        },
                        new {
                            subcategory = "Faces",
                            subcategoryId = 10,
                            name = "Collectible Faces",
                            shortName = "Faces",
                        },
                        new {
                            subcategory = "Gear",
                            subcategoryId = 5,
                            name = "Collectible Gear",
                            shortName = "Gear",
                        },
                    },
                    isSearchable = true,
                },
                new {
					category = "Clothing",
					categoryId = 3,
					name = "Clothing",
					orderIndex = 6,
					subcategories = new List<dynamic>()
					{
						new {
							subcategory = "Clothing",
							subcategoryId = 3,
							name = "All Clothing",
							shortName = "All",
						},
						new {
							subcategory = "Shirts",
							subcategoryId = 12,
							name = "Shirts",
							shortName = (string?)null,
						},
						new {
							subcategory = "Tshirts",
							subcategoryId = 13,
							name = "T-Shirts",
							shortName = (string?)null,
						},
						new {
							subcategory = "Pants",
							subcategoryId = 14,
							name = "Pants",
							shortName = (string?)null,
						},
						new {
							subcategory = "Bundles",
							subcategoryId = 37,
							name = "Bundles",
							shortName = (string?)null,
						},
					},
					isSearchable = true,
				},
				new {
					category = "BodyParts",
					categoryId = 4,
					name = "Body Parts",
					orderIndex = 7,
					subcategories = new List<dynamic>()
					{
						new {
							subcategory = "BodyParts",
							subcategoryId = 4,
							name = "All Body Parts",
							shortName = "All",
						},
						new {
							subcategory = "Heads",
							subcategoryId = 15,
							name = "Heads",
							shortName = (string?)null,
						},
						new {
							subcategory = "Faces",
							subcategoryId = 10,
							name = "Faces",
							shortName = (string?)null,
						},
						new{
							subcategory = "Bundles",
							subcategoryId = 37,
							name = "Bundles",
							shortName = (string?)null,
						},
					},
					isSearchable = true,
				},
				new {
					category = "Gear",
					categoryId = 5,
					name = "Gear",
					orderIndex = 8,
					subcategories = new List<dynamic>()
					{
						new {
							subcategory = "Gear",
							subcategoryId = 0,
							name = "All Gear",
							shortName = "All",
						},
						new {
							subcategory = "Building",
							subcategoryId = 8,
							name = "Building",
							shortName = (string?)null,
						},
						new {
							subcategory = "Explosive",
							subcategoryId = 3,
							name = "Explosive",
							shortName = (string?)null,
						},
						new {
							subcategory = "Melee",
							subcategoryId = 1,
							name = "Melee",
							shortName = (string?)null,
						},
						new {
							subcategory = "Musical",
							subcategoryId = 6,
							name = "Musical",
							shortName = (string?)null,
						},
						new {
							subcategory = "Navigation",
							subcategoryId = 5,
							name = "Navigation",
							shortName = (string?)null,
						},
						new {
							subcategory = "PowerUp",
							subcategoryId = 4,
							name = "Power Up",
							shortName = (string?)null,
						},
						new {
							subcategory = "Ranged",
							subcategoryId = 2,
							name = "Ranged",
							shortName = (string?)null,
						},
						new {
							subcategory = "Social",
							subcategoryId = 7,
							name = "Social",
							shortName = (string?)null,
						},
						new {
							subcategory = "Transport",
							subcategoryId = 9,
							name = "Transport",
							shortName = (string?)null,
						},
					},
					isSearchable = true,
				},
				new {
					category = "Accessories",
					categoryId = 11,
					name = "Accessories",
					orderIndex = 9,
					subcategories = new List<dynamic>()
					{
						new {
							subcategory = "Accessories",
							subcategoryId = 19,
							name = "All Accessories",
							shortName = "All",
						},
						new {
							subcategory = "Hats",
							subcategoryId = 9,
							name = "Hats",
							shortName = (string?)null,
						},
						new {
							subcategory = "HairAccessories",
							subcategoryId = 20,
							name = "Hair",
							shortName = (string?)null,
						},
						new {
							subcategory = "FaceAccessories",
							subcategoryId = 21,
							name = "Face",
							shortName = (string?)null,
						},
						new {
							subcategory = "NeckAccessories",
							subcategoryId = 22,
							name = "Neck",
							shortName = (string?)null,
						},
						new {
							subcategory = "ShoulderAccessories",
							subcategoryId = 23,
							name = "Shoulder",
							shortName = (string?)null,
						},
						new {
							subcategory = "FrontAccessories",
							subcategoryId = 24,
							name = "Front",
							shortName = (string?)null,
						},
						new {
							subcategory = "BackAccessories",
							subcategoryId = 25,
							name = "Back",
							shortName = (string?)null,
						},
						new{
							subcategory = "WaistAccessories",
							subcategoryId = 26,
							name = "Waist",
							shortName = (string?)null,
						},
					},
					isSearchable = true,
				},
				/*
				{
					category = "AvatarAnimations",
					categoryId = 12,
					name = "Avatar Animations",
					orderIndex = 10,
					subcategories = [
						{
							subcategory = "AnimationBundles",
							subcategoryId = 38,
							name = "Bundles",
							shortName = (string?)null,
						},
						{
							subcategory = "EmoteAnimations",
							subcategoryId = 39,
							name = "Emotes",
							shortName = (string?)null,
						},
					],
					isSearchable = true,
				},
				*/
            },
            genres = new List<dynamic>()
            {
				new { genre = 13, name = "Building", isSelected = false },
				new { genre = 5, name = "Horror", isSelected = false },
				new { genre = 1, name = "Town and City", isSelected = false },
				new { genre = 11, name = "Military", isSelected = false },
				new { genre = 9, name = "Comedy", isSelected = false },
				new { genre = 2, name = "Medieval", isSelected = false },
				new { genre = 7, name = "Adventure", isSelected = false },
				new { genre = 3, name = "Sci-Fi", isSelected = false },
				new { genre = 6, name = "Naval", isSelected = false },
				new { genre = 14, name = "FPS", isSelected = false },
				new { genre = 15, name = "RPG", isSelected = false },
				new { genre = 8, name = "Sports", isSelected = false },
				new { genre = 4, name = "Fighting", isSelected = false },
				new { genre = 10, name = "Western", isSelected = false },
			},
			sortMenu = new 
			{
				sortOptions = new List<dynamic>()
				{
					new 
					{
						sortType = 0,
						sortOrder = 2,
						name = "Relevance",
						isSelected = false,
						hasSubMenu = false,
						isPriceRelated = false,
					},
					/*
					{
						sortType = 1,
						sortOrder = 2,
						name = "Most Favorited",
						isSelected = false,
						hasSubMenu = true,
						isPriceRelated = false,
					},
					{
						sortType = 2,
						sortOrder = 2,
						name = "Bestselling",
						isSelected = false,
						hasSubMenu = true,
						isPriceRelated = false,
					},
					*/
					new {
						sortType = 3,
						sortOrder = 2,
						name = "Recently Updated",
						isSelected = false,
						hasSubMenu = false,
						isPriceRelated = false,
					},
					new {
						sortType = 5,
						sortOrder = 2,
						name = "Price (High to Low)",
						isSelected = false,
						hasSubMenu = false,
						isPriceRelated = true,
					},
					new {
						sortType = 4,
						sortOrder = 1,
						name = "Price (Low to High)",
						isSelected = false,
						hasSubMenu = false,
						isPriceRelated = true,
					},
				},
				sortAggregations = new List<dynamic>(){
					new {
						sortAggregation = 5,
						name = "All Time",
						isSelected = false,
						hasSubMenu = false,
						isPriceRelated = false,
					},
					new {
						sortAggregation = 3,
						name = "Past Week",
						isSelected = false,
						hasSubMenu = false,
						isPriceRelated = false,
					},
					new {
						sortAggregation = 1,
						name = "Past Day",
						isSelected = false,
						hasSubMenu = false,
						isPriceRelated = false,
					},
				},
			},
			creatorFilters = new List<dynamic>() {
				new { userId = 0, name = "All Creators", isSelected = false },
				new { userId = 1, name = "Roblox", isSelected = false },
			},
			priceFilters = new List<dynamic>() {
				new {
					currencyType = 0,
					name = "Any Price",
					excludePriceSorts = false,
				},
				new { currencyType = 3, name = "R$?", excludePriceSorts = false },
				new { currencyType = 5, name = "Free", excludePriceSorts = true },
			},
        };
    }

    [HttpGet("catalog/metadata")]
    public dynamic GetCatalogMetadata()
    {
	    return new
	    {
		    numberOfCatalogItemsToDisplayOnSplash = 25,
		    numberOfCatalogItemsToDisplayOnSplashOnPhone = 15,
		    isCatalogSortsFromApiEnabled = false,
		    is3dInEachItemCardAbTestingEnabled = false,
		    is3dInEachItemCardEnabled = false,
		    timeoutOn3dThumbnailRequestInMs = 0,
		    isNewRobuxIconEnabled = true,
		    isPremiumPriceOnItemTilesEnabled = false,
		    isPremiumIconOnItemTilesEnabled = true,
		    isPremiumSortEnabled = true
	    };
    }

    [HttpGet("recommendations/asset/{assetTypeId}")]
    public async Task<dynamic> GetRecommendations(Models.Assets.Type assetTypeId, long contextAssetId, int numItems)
    {
	    var result = await services.assets.GetRecommendedItems(assetTypeId, contextAssetId, numItems);
	    return new
	    {
		    data = result.Select(c => new
		    {
			    item = new
			    {
				    assetId = c.assetId,
				    name = c.name,
				    price = c.price,
				    premiumPrice = (int?) null,
				    absoluteUrl = $"/catalog/{c.assetId}/--",
			    },
			    creator = new
			    {
				    creatorId = c.creatorId,
				    creatorType = c.creatorType,
				    name = c.creatorName,
				    creatorProfileLink = c.creatorType == CreatorType.User
					    ? $"/users/{c.creatorId}/profile"
					     : $"/groups/{c.creatorId}/--",
			    },
			    product = new
			    {
				    id = c.assetId,
				    priceInRobux = c.price,
				    isForsale = c.isForSale,
				    isPublicDomain = false, // todo
				    isResellable = c.isLimited || c.isLimitedUnique,
				    c.isLimited,
				    c.isLimitedUnique,
				    isRental = false,
				    bcRequirement = 0,
				    totalPrivateSales = 0, // todo = what is this?
				    offsaleDeadline = c.offsaleDeadline,
				    noPriceText = (c.isLimited || c.isLimitedUnique && !c.isForSale) ? "No Resellers" : null,
				    // below is intentionally empty
				    sellerId = 0,
				    sellerName = (string?)null,
				    lowestPrivateSaleUserAssetId = (int?)null,
				    isXboxExclusiveItem = false,
			    },
		    }),
	    };
    }

    [HttpPost("catalog/items/details")]
    public async Task<RobloxCollection<MultiGetEntry>> MultiGetItemDetails([Required, FromBody] WebsiteModels.Catalog.MultiGetRequest request)
    {
	    var result = await services.assets.MultiGetInfoById(request.items.Select(c => c.id));
	    return new RobloxCollection<MultiGetEntry>()
	    {
		    data = result,
	    };
    }

    [HttpGet("search/items")]
    public async Task<SearchResponse> SearchItems(string? category, string? subcategory, string? sortType, string? keyword, string? cursor, int limit = 10, CreatorType? creatorType = null, long? creatorTargetId = null, bool includeNotForSale = false, string? _genreFilterCsv = null)
    {
	    var include18Plus = userSession != null && await services.users.Is18Plus(userSession.userId);
	    var request = new CatalogSearchRequest()
	    {
		    category = category,
		    keyword = keyword,
		    subcategory = subcategory,
		    sortType = sortType,
		    cursor = cursor,
		    limit = limit,
		    creatorType = creatorType,
		    creatorTargetId = creatorTargetId,
		    includeNotForSale = includeNotForSale,
		    genres = _genreFilterCsv?.Split(",").Select(Enum.Parse<Genre>),
		    include18Plus = include18Plus,
	    };
	    if (request.limit is > 100 or < 1) request.limit = 10;
	    return await services.assets.SearchCatalog(request);
    }

    [HttpGet("recommendations/metadata")]
    public dynamic GetRecommendationsMetadata(string? page)
    {
	    var displayed = 0;
	    var retrieved = 0;
	    var subject = "assets";
	    
	    if (page == "Avatar")
	    {
		    displayed = 5;
		    retrieved = 50;
		    subject = "avatar";
	    }
	    else if (page == "Inventory")
	    {
		    displayed = 6;
		    retrieved = 50;
		    subject = "user-inventory";
	    }
	    else if (page == "CatalogItem")
	    {
		    displayed = 7;
		    retrieved = 50;
	    }

	    return new
	    {
		    numOfRecommendationsDisplayed = displayed,
		    numOfRecommendationsRetrieved = retrieved,
		    subject,
		    isV2EndpointEnabled = false,
	    };
    }

    [HttpGet("favorites/users/{userId:long}/assets/{assetId:long}/favorite")]
    public async Task<FavoriteEntry?> GetFavoriteStatus(long userId, long assetId)
    {
	    return await services.assets.GetFavoriteStatus(safeUserSession.userId, assetId);
    }

    [HttpPost("favorites/users/{userId:long}/assets/{assetId:long}/favorite")]
    public async Task CreateFavorite(long userId, long assetId)
    {
	    await services.assets.CreateFavorite(safeUserSession.userId, assetId);
    }

    [HttpDelete("favorites/users/{userId:long}/assets/{assetId:long}/favorite")]
    public async Task DeleteFavorite(long userId, long assetId)
    {
	    await services.assets.DeleteFavorite(safeUserSession.userId, assetId);
    }

    [HttpGet("asset-to-category")]
    public dynamic GetAssetToCategory()
    {
	    return new Dictionary<int, int>()
	    {
		    {8,11},
		    {41,11},
		    {42,11},
		    {43,11},
		    {44,11},
		    {45,11},
		    {46,11},
		    {47,11},
		    {53,12},
		    {55,12},
		    {50,12},
		    {52,12},
		    {51,12},
		    {54,12},
		    {48,12},
		    {18,4},
		    {17,4},
		    {19,5},
		    {12,3},
		    {11,3},
		    {2,3},
		    {3,9},
		    {62,14},
		    {13,8},
		    {10,6},
		    {40,10},
		    {38,7},
	    };
    }
    
    [HttpGet("asset-to-subcategory")]
    public dynamic GetAssetToSubcategory()
    {
	    return new Dictionary<int, int>()
	    {
		    {2,13},
		    {3,16},
		    {8,9},
		    {10,6},
		    {11,12},
		    {12,14},
		    {13,8},
		    {17,15},
		    {18,10},
		    {19,5},
		    {38,7},
		    {40,18},
		    {41,20},
		    {42,21},
		    {43,22},
		    {44,23},
		    {45,24},
		    {46,25},
		    {47,28},
		    {48,28},
		    {50,30},
		    {51,31},
		    {52,32},
	    };
    }
}