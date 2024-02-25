using System.Text.Json;
using Dapper;
using Roblox.Dto.Users;
using Roblox.Models.Users;

namespace Roblox.Services;

public class InventoryService : ServiceBase, IService
{
    public async Task<IEnumerable<InventoryPrivacyEntry>> MultiGetInventoryPrivacy(IEnumerable<long> userIds)
    {
        var sql = new SqlBuilder();
        var t = sql.AddTemplate(
            "SELECT user_id as userId, inventory_privacy as privacy FROM user_settings /**where**/ LIMIT 10000");
        foreach (var id in userIds)
        {
            sql.OrWhere("user_id = " + id);
        }

        return await db.QueryAsync<InventoryPrivacyEntry>(t.RawSql, t.Parameters);
    }

    public async Task<IEnumerable<CollectibleItemEntry>> GetCollectibleInventory(long userId, Models.Assets.Type? type,
        string sortOrder, int limit, int offset)
    {
        var sql = new SqlBuilder();
        var t = sql.AddTemplate(
            "SELECT user_asset.id as userAssetId, serial as serialNumber, user_asset.asset_id as assetId, asset.recent_average_price as recentAveragePrice, asset.price_robux as originalPrice, asset.serial_count as assetStock, asset.asset_type as assetTypeId, asset.name as name FROM user_asset INNER JOIN asset ON asset.id = user_asset.asset_id /**where**/ /**orderby**/ LIMIT :limit OFFSET :offset", new
            {
                limit = limit,
                offset = offset,
                user_id = userId,
            });
        sql.OrderBy("user_asset.id " + (sortOrder == "desc" ? "desc" : "asc"));
        sql.Where("asset.is_limited /*AND NOT asset.is_for_sale*/ AND user_asset.user_id = :user_id", new {user_id = userId});
        if (type != null)
        {
            sql.Where("asset.asset_type = :type", new
            {
                type = (int) type,
            });
        }

        return await db.QueryAsync<CollectibleItemEntry>(t.RawSql, t.Parameters);
    }

    public async Task<int> CountInventory(long userId, Models.Assets.Type? type)
    {
        if (type == null)
            return (await db.QuerySingleOrDefaultAsync<Dto.Total>(
                "SELECT COUNT(*) as total FROM user_asset WHERE user_id = :id", new {id = userId})).total;
        return (await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) as total FROM user_asset INNER JOIN asset a ON user_asset.asset_id = a.id WHERE user_id = :id AND a.asset_type = :type", new {id = userId, type = type})).total;
    }
    
    public async Task<IEnumerable<InventoryEntry>> GetInventory(long userId, Models.Assets.Type? type,
        string sortOrder, int limit, int offset)
    {
        var sql = new SqlBuilder();
        var t = sql.AddTemplate(
            "SELECT user_asset.id as userAssetId, serial as serialNumber, user_asset.asset_id as assetId, asset.recent_average_price as recentAveragePrice, asset.price_robux as originalPrice, asset.serial_count as assetStock, asset.asset_type as assetTypeId, asset.name as name, asset.is_limited as isLimited, asset.is_limited_unique as isLimitedUnique, asset.creator_id as creatorId, asset.creator_type as creatorType, (CASE WHEN asset.creator_type = 1 THEN u.username ELSE g.name END) as creatorName FROM user_asset INNER JOIN asset ON asset.id = user_asset.asset_id LEFT JOIN \"user\" u ON u.id = asset.creator_id AND asset.creator_type = 1 LEFT JOIN \"group\" g ON g.id = asset.creator_id AND asset.creator_type = 2 /**where**/ /**orderby**/ LIMIT :limit OFFSET :offset", new
            {
                limit = limit,
                offset = offset,
                user_id = userId,
            });
        sql.OrderBy("user_asset.id " + (sortOrder == "desc" ? "desc" : "asc"));
        sql.Where("user_asset.user_id = :user_id", new {user_id = userId});
        if (type != null)
        {
            sql.Where("asset.asset_type = :type", new
            {
                type = (int) type,
            });
        }

        return await db.QueryAsync<InventoryEntry>(t.RawSql, t.Parameters);
    }

    private bool CanAddTypeToCollections(Models.Assets.Type assetType)
    {
        return assetType switch
        {
            Models.Assets.Type.Hat => true,
            Models.Assets.Type.HairAccessory => true,
            Models.Assets.Type.FaceAccessory => true,
            Models.Assets.Type.NeckAccessory => true,
            Models.Assets.Type.ShoulderAccessory => true,
            Models.Assets.Type.FrontAccessory => true,
            Models.Assets.Type.BackAccessory => true,
            Models.Assets.Type.WaistAccessory => true,
            _ => false,
        };
    }
    
    public async Task<IEnumerable<long>> GetCollections(long userId)
    {
        var result = await redis.StringGetAsync("user_collections_json_" + userId);
        if (result == null)
            return ArraySegment<long>.Empty;
        var parsed = JsonSerializer.Deserialize<IEnumerable<long>>(result);
        if (parsed == null)
            return ArraySegment<long>.Empty;
        var ids = parsed.ToList();
        using var assets = ServiceProvider.GetOrCreate<AssetsService>(this);
        var details = (await assets.MultiGetInfoById(ids)).Where(c => CanAddTypeToCollections(c.assetType)).Select(c => c.id).ToList();
        // get order of original list
        var newList = new List<long>();
        foreach (var oldId in ids)
        {
            var inDetails = details.IndexOf(oldId) != -1;
            if (inDetails && !newList.Contains(oldId))
            {
                newList.Add(oldId);
                if (newList.Count == 6)
                    break;
            }
        }

        return newList;
    }

    public async Task SetCollections(long userId, IEnumerable<long> assetIds)
    {
        assetIds = assetIds.Distinct().Take(64);
        using var assets = ServiceProvider.GetOrCreate<AssetsService>(this);
        var filteredIds = (await assets.MultiGetInfoById(assetIds)).Where(c => CanAddTypeToCollections(c.assetType))
            .Select(c => c.id);
        
        var str = JsonSerializer.Serialize(filteredIds);
        await redis.StringSetAsync("user_collections_json_" + userId, str);
    }

    public async Task<IEnumerable<OwnershipEntry>> GetOwners(long assetId, string sortOrder, int offset, int limit)
    {
        var result = await db.QueryAsync<OwnershipEntryDb>(
            "SELECT ua.id, ua.serial as serialNumber, u.id as userId, u.username as username, ua.created_at as created, ua.updated_at as updated FROM user_asset AS ua INNER JOIN \"user\" AS u ON u.id = ua.user_id WHERE ua.asset_id = :asset_id ORDER BY ua.id " +
            (sortOrder.ToLower() == "desc"
                ? "desc"
                : "asc") + " LIMIT :limit OFFSET :offset", new
            {
                asset_id = assetId,
                limit,
                offset,
            });
        return result.Select(c => new OwnershipEntry()
        {
            id = c.id,
            serialNumber = c.serialNumber,
            created = c.created,
            owner = new()
            {
                id = c.userId,
                name = c.username,
            },
            updated = c.updated,
        });
    }
    
            public async Task<bool> CanViewInventory(long userId, long contextUserId = 0)
        {
            var result = await MultiCanViewInventory(new[] { userId }, contextUserId);
            return result.First().canView;
        }

        public async Task<IEnumerable<Roblox.Dto.Users.CanViewInventoryEntry>> MultiCanViewInventory(IEnumerable<long> userIds, long contextUserId = 0)
        {
            // This function is big but not too hard to follow, just a lot of lists that get re-purposed
            using var friends = ServiceProvider.GetOrCreate<FriendsService>();
            using var users = ServiceProvider.GetOrCreate<UsersService>();

            var toQuery = userIds.Distinct().ToList();
            var results = new List<Dto.Users.CanViewInventoryEntry>();
            var ids = toQuery.ToList();
            foreach (var userId in ids)
            {
                if (userId == contextUserId)
                {
                    results.Add(new Dto.Users.CanViewInventoryEntry()
                    {
                        userId = userId,
                        canView = true,
                    });
                    toQuery.Remove(userId);
                }
            }

            // Early exit
            if (toQuery.Count == 0) return results;
            
            // remove terminated users
            var info = await users.MultiGetAccountStatus(ids);
            foreach (var status in info.Where(c => c.IsDeleted()))
            {
                results.Add(new Dto.Users.CanViewInventoryEntry()
                {
                    userId = status.userId,
                    canView = false,
                });
                toQuery.Remove(status.userId);
            }
            if (toQuery.Count == 0) return results;

            // Privacy query
            var privacyResults = (await MultiGetInventoryPrivacy(toQuery)).ToList();
            foreach (var privacy in privacyResults.ToList())
            {
                if (privacy.privacy == InventoryPrivacy.AllUsers)
                {
                    results.Add(new CanViewInventoryEntry()
                    {
                        canView = true,
                        userId = privacy.userId,
                    });
                    privacyResults.Remove(privacy);
                }
                else if (privacy.privacy == InventoryPrivacy.AllAuthenticatedUsers)
                {
                    results.Add(new CanViewInventoryEntry()
                    {
                        canView = contextUserId != 0,
                        userId = privacy.userId,
                    });
                    privacyResults.Remove(privacy);
                }
                else if (privacy.privacy == InventoryPrivacy.NoOne)
                {
                    results.Add(new CanViewInventoryEntry()
                    {
                        canView = false,
                        userId = privacy.userId,
                    });
                    privacyResults.Remove(privacy);
                }
                else
                {
                    // Followers, Followings, or Friends
                    if (contextUserId == 0)
                    {
                        results.Add(new CanViewInventoryEntry()
                        {
                            canView = false,
                            userId = privacy.userId,
                        });
                        privacyResults.Remove(privacy);
                    }

                }
            }

            // Early exit - we don't have to do friends query
            if (privacyResults.Count == 0) return results;

            var friendsStatus =
                await friends.MultiGetFriendshipStatus(contextUserId, privacyResults.Select(c => c.userId));

            var checkFollowers = new List<long>();
            foreach (var friend in friendsStatus)
            {
                var privacyStatus = privacyResults.Find(c => c.userId == friend.id);
                if (privacyStatus == null) throw new Exception("No privacy entry for " + friend.id);

                if (friend.status == "Friends")
                {
                    // Friend, so allow viewing
                    privacyResults.Remove(privacyStatus);
                    results.Add(new()
                    {
                        canView = true,
                        userId = friend.id,
                    });
                }
                else
                {
                    // If you need to be a friend to view inventory, return false since user is not friend
                    if (privacyStatus.privacy != InventoryPrivacy.Friends)
                    {
                        checkFollowers.Add(friend.id);
                    }
                    else
                    {
                        results.Add(new()
                        {
                            canView = false,
                            userId = friend.id,
                        });
                    }
                }
            }

            if (checkFollowers.Count == 0) return results;
            foreach (var user in checkFollowers)
            {
                var privacy = privacyResults.Find(c => c.userId == user)!;
                if (privacy.privacy == InventoryPrivacy.FriendsAndFollowing)
                {
                    var isUserFollowingCtx = await friends.IsOneFollowingTwo(user, contextUserId);
                    if (isUserFollowingCtx)
                    {
                        results.Add(new CanViewInventoryEntry()
                        {
                            canView = true,
                            userId = user,
                        });
                    }
                }
                else if (privacy.privacy == InventoryPrivacy.FriendsFollowingAndFollowers)
                {
                    var canView = await friends.IsOneFollowingTwo(user, contextUserId);
                    if (!canView)
                    {
                        canView = await friends.IsOneFollowingTwo(contextUserId, user);
                    }

                    results.Add(new CanViewInventoryEntry()
                    {
                        canView = canView,
                        userId = user,
                    });
                }
            }

            return results;
        }

        public bool IsThreadSafe()
        {
            return true;
        }

        public bool IsReusable()
        {
            return false;
        }
}