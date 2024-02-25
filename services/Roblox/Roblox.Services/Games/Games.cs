using Dapper;
using Roblox.Dto;
using Roblox.Dto.Games;
using Roblox.Libraries;
using Roblox.Models.Assets;
using Roblox.Services.Exceptions;
using Type = Roblox.Models.Assets.Type;

namespace Roblox.Services;

public class GamesService : ServiceBase, IService
{
    public async Task<long> GetMaxPlayerCount(long placeId)
    {
        var result = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT asset_place.max_player_count AS total FROM asset_place WHERE asset_id = :id LIMIT 1", new
            {
                id = placeId,
            });
        return result?.total ?? 0;
    }

    public async Task<long> GetRootPlaceId(long universeId)
    {
        var details = await MultiGetUniverseInfo(new []{universeId});
        var arr = details.ToArray();
        if (arr.Length == 0)
            throw new RobloxException(400, 0, "Invalid universe ID");
        return arr[0].rootPlaceId;
    }
    
    public async Task<long> GetUniverseId(long placeId)
    {
        var details = await MultiGetPlaceDetails(new []{placeId});
        var arr = details.ToArray();
        if (arr.Length == 0)
            throw new RobloxException(400, 0, "Invalid place ID");
        return arr[0].universeId;
    }
    
    public async Task<IEnumerable<MultiGetUniverseEntry>> MultiGetUniverseInfo(IEnumerable<long> universeIds)
    {
        var ids = universeIds.ToArray();
        if (!ids.Any())
            return Array.Empty<MultiGetUniverseEntry>();
        
        var build = new SqlBuilder();
        var temp = build.AddTemplate(
            "SELECT universe.id, universe.root_asset_id as rootPlaceId, asset.name, asset.description, asset.asset_genre as genre, asset.created_at as created, asset.updated_at as updated, asset_place.max_player_count as maxPlayers, asset_place.visit_count as visits, asset_place.is_vip_enabled as createVipServersAllowed, asset.price_robux as price, asset.creator_id as creatorId, asset.creator_type as creatorType, (SELECT COUNT(*) as playing FROM asset_server_player WHERE asset_id = universe.root_asset_id), (case when \"asset\".creator_type = 1 then \"user\".username else \"group\".name end) as creatorName FROM universe INNER JOIN asset ON asset.id = universe.root_asset_id INNER JOIN asset_place ON asset_place.asset_id = universe.root_asset_id LEFT JOIN \"group\" ON \"group\".id = asset.creator_id LEFT JOIN \"user\" ON \"user\".id = asset.creator_id /**where**/ LIMIT 1000");
        foreach (var id in ids)
        {
            build.OrWhere("universe.id = " + id);
        }

        var result = (await db.QueryAsync<MultiGetUniverseEntry>(temp.RawSql, temp.Parameters)).ToList();
        using var assets = ServiceProvider.GetOrCreate<AssetsService>(this);

        var favorites = await Task.WhenAll(result.Select(c => assets.CountFavorites(c.rootPlaceId)));
        for (var i = 0; i < result.Count; i++)
        {
            result[i].favoritedCount = favorites[i];
        }
        return result;
    }

    public async Task<PlayEntry?> GetOldestPlay(long userId)
    {
        var oldest = await db.QuerySingleOrDefaultAsync<PlayEntry?>(
            "SELECT created_at as createdAt, ended_at as endedAt, asset_id as placeId FROM asset_play_history WHERE user_id = :user_id ORDER BY created_at LIMIT 1", new
            {
                user_id = userId,
            });
        return oldest;
    }

    public async Task<IEnumerable<PlayEntry>> GetRecentGamePlays(long userId, TimeSpan period)
    {
        var date = DateTime.UtcNow.Subtract(period);
        return await db.QueryAsync<PlayEntry>(
            "SELECT created_at as createdAt, ended_at as endedAt, asset_id as placeId FROM asset_play_history WHERE user_id = :user_id AND created_at >= :t", new
            {
                t = date,
                user_id = userId,
            });
    }

    public async Task<IEnumerable<long>> GetRecentGames(long userId, int limit)
    {
        var result = await db.QueryAsync(
            "SELECT asset_play_history.id, asset_id FROM asset_play_history INNER JOIN asset ON asset.id = asset_play_history.asset_id WHERE user_id = :user_id AND asset.moderation_status = :mod_status ORDER BY asset_play_history.id DESC", new
            {
                user_id = userId,
                mod_status = ModerationStatus.ReviewApproved,
            });

        return result.Select(c => (long) c.asset_id).Distinct().Take(limit);
    }

    public async Task<int> GetPlayerCount(long placeId)
    {
        var query = await db.QuerySingleOrDefaultAsync<Total>(
            "select count(*) as total FROM asset_server_player WHERE asset_server_player.asset_id = :id", new
            {
                id = placeId,
            });
        return query.total;
    }
    
    public async Task<int> GetVisitCount(long placeId)
    {
        var query = await db.QuerySingleOrDefaultAsync<Total>(
            "select asset_place.visit_count AS total FROM asset_place WHERE asset_place.asset_id = :id", new
            {
                id = placeId,
            });
        return query.total;
    }

    public async Task<IEnumerable<GameListEntry>> GetGamesList(long? contextUserId, string? sortToken, int maxRows, Genre? genre, string? keyword)
    {
        var query = new SqlBuilder();
        var temp = query.AddTemplate(
            "SELECT asset.creator_id as creatorId, asset.creator_type as creatorTypeId, universe_asset.universe_id as universeId, asset.name, asset.id as placeId, asset.description as gameDescription, asset.asset_genre as genre, (select count(*) as playerCount FROM asset_server_player WHERE asset_server_player.asset_id = asset.id), (select count(*) from asset_favorite where asset_id = asset_place.asset_id) as favorite_count, (case when asset.creator_type = 1 then \"user\".username else \"group\".name end) as creatorName, asset_place.visit_count as visitCount, (select count(*) as totalUpVotes from asset_vote where asset_id = asset_place.asset_id and type = :upvote), (select count(*) as totalDownVotes from asset_vote where asset_id = asset_place.asset_id and type = :downvote) FROM asset INNER JOIN universe_asset ON universe_asset.asset_id = asset.id INNER JOIN asset_place ON asset_place.asset_id = asset.id LEFT JOIN \"group\" ON \"group\".id = asset.creator_id AND asset.creator_type = 2 LEFT JOIN \"user\" ON \"user\".id = asset.creator_id AND asset.creator_type = 1 /**where**/ /**orderby**/ LIMIT :limit",
            new
            {
                limit = maxRows,
                upvote = AssetVoteType.Upvote,
                downvote = AssetVoteType.Downvote,
            });
        // wheres that apply to all filters
        query.Where("asset.moderation_status = :mod_status", new
        {
            mod_status = ModerationStatus.ReviewApproved,
        });
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query.Where("asset.name ILIKE :keyword", new
            {
                keyword = keyword + "%",
            });
        }
        
        if (genre != null && genre != Genre.All && Enum.IsDefined(genre.Value))
        {
            query.Where("asset.asset_genre = :genre", new
            {
                genre = (int) genre,
            });
        }

        var is18Plus = false;
        if (contextUserId != null)
        {
            using var us = ServiceProvider.GetOrCreate<UsersService>();
            is18Plus = await us.Is18Plus(contextUserId.Value);
        }
        if (!is18Plus)
            query.Where("NOT asset.is_18_plus");

        List<long>? sortOrder = null;
        var sortRequired = true;
        switch (sortToken?.ToLower())
        {
            case "recent":
                if (contextUserId is 0 or null)
                    throw new RobloxException(401, 0, "Unauthorized");
                
                sortOrder = (await GetRecentGames(contextUserId.Value, maxRows)).ToList();
                foreach (var item in sortOrder)
                {
                    query.OrWhere("asset.id = " + item);
                }
                break;
            case "recentlyupdated":
                query.OrderBy("asset.updated_at DESC");
                sortRequired = false;
                break;
            case "recentlycreated":
                query.OrderBy("asset.created_at DESC");
                sortRequired = false;
                break;
            case "mostfavorited":
                // query.Where("");
                query.OrderBy("favorite_count DESC");
                sortRequired = false;
                break;
            default:
                // popular and default are same
                query.OrderBy("playerCount DESC, asset_place.visit_count DESC");
                break;
        }

        var result = await db.QueryAsync<GameListEntry>(temp.RawSql, temp.Parameters);
        // If required, use custom sort
        if (sortOrder != null)
        {
            var newResults = new List<GameListEntry>();
            var oldResult = result.ToList();
            foreach (var id in sortOrder)
            {
                var row = oldResult.FirstOrDefault(c => c.placeId == id); 
                if (row != null)
                    newResults.Add(row);
            }

            result = newResults;
        }
        else if (sortRequired)
        {
            // Try to sort by highest player count - should be done by sql but I can't test it right now
            var newResults = result.ToList();
            newResults.Sort((a, b) =>
            {
                return a.playerCount > b.playerCount ? -1 : a.playerCount == b.playerCount ? 
                    (a.visitCount > b.visitCount ? -1 : a.visitCount == b.visitCount ? 0 : 1) 
                    : 1;
            });
            result = newResults;
        }

        return result;
    }

    public async Task SetMaxPlayerCount(long placeId, int maxPlayerCount)
    {
        if (maxPlayerCount < 10)
            throw new RobloxException(400, 0, "Max player count cannot be below 10");
        if (maxPlayerCount > 20)
            throw new RobloxException(400, 0, "Max player count cannot exceed 20");
        
        await db.ExecuteAsync("UPDATE asset_place SET max_player_count = :max WHERE asset_id = :id", new
        {
            id = placeId,
            max = maxPlayerCount,
        });
    }

    public async Task<IEnumerable<PlaceEntry>> MultiGetPlaceDetails(IEnumerable<long> placeIds)
    {
        var ids = placeIds.Distinct().ToArray();
        if (ids.Length == 0)
            return ArraySegment<PlaceEntry>.Empty;

        var query = new SqlBuilder();
        var temp = query.AddTemplate(
            "SELECT asset.id as universeRootPlaceId, asset.creator_id as builderId, asset.creator_type as builderType, universe_asset.universe_id as universeId, asset.name, asset.id as placeId, asset.description as description, asset.asset_genre as genre, (select count(*) as playerCount FROM asset_server_player WHERE asset_server_player.asset_id = asset.id), (case when \"asset\".creator_type = 1 then \"user\".username else \"group\".name end) as builder, asset.created_at as created, asset.updated_at as updated, asset_place.max_player_count as maxPlayerCount, asset.asset_genre as genre, asset.moderation_status as moderationStatus FROM asset INNER JOIN universe_asset ON universe_asset.asset_id = asset.id INNER JOIN asset_place ON asset_place.asset_id = asset.id LEFT JOIN \"group\" ON \"group\".id = asset.creator_id AND asset.creator_type = 2 LEFT JOIN \"user\" ON \"user\".id = asset.creator_id AND asset.creator_type = 1 /**where**/ /**orderby**/ LIMIT 100");

        foreach (var id in ids)
        {
            query.OrWhere("(asset.asset_type = " + (int) Type.Place + " AND asset.id = " + id + ")");
        }

        return await db.QueryAsync<PlaceEntry>(temp.RawSql, temp.Parameters);
    }

    public async Task<IEnumerable<GamesForCreatorEntry>> GetGamesForType(CreatorType creatorType, long creatorId, int limit,
        int offset, string? sort, string? accessFilter)
    {
        var qu = await db.QueryAsync<GamesForCreatorEntryDb>(
            "SELECT u.id, a.name, a.description, u.root_asset_id as rootAssetId, ap.visit_count as visitCount, a.created_at as created, a.updated_at as updated FROM universe AS u INNER JOIN asset a ON a.id = u.root_asset_id INNER JOIN asset_place ap ON ap.asset_id = u.root_asset_id WHERE u.creator_type = :type AND u.creator_id = :id LIMIT :limit OFFSET :offset", new
            {
                type = creatorType,
                id = creatorId,
                limit,
                offset,
            });
        return qu.Select(c => new GamesForCreatorEntry()
        {
            id = c.id,
            created = c.created,
            description = c.description,
            name = c.name,
            placeVisits = c.visitCount,
            rootPlace = new()
            {
                id = c.rootAssetId,
            },
            updated = c.updated,
        });
    }

    public async Task<IEnumerable<GameMediaEntry>> GetGameMedia(long placeId)
    {
        return await db.QueryAsync<GameMediaEntry>(
            "SELECT asset_type as assetTypeId, media_asset_id as imageId, media_video_hash as videoHash, media_video_title as videoTitle, is_approved as isApproved FROM asset_media WHERE asset_id = :id",
            new {id = placeId});
    }

    public async Task<CreateUniverseResponse> CreateUniverse(long rootPlaceId)
    {
        return await InTransaction(async _ =>
        {
            var creatorInfo =
                await db.QuerySingleOrDefaultAsync("SELECT creator_id, creator_type FROM asset WHERE id = :id",
                    new {id = rootPlaceId});
            var uni = await InsertAsync("universe", new
            {
                root_asset_id = rootPlaceId,
                is_public = true,
                creator_id = (long) creatorInfo.creator_id,
                creator_type = (int) creatorInfo.creator_type,
            });
            await InsertAsync("universe_asset", new
            {
                asset_id = rootPlaceId,
                universe_id = uni,
            });
            return new CreateUniverseResponse()
            {
                universeId = uni,
            };
        });
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