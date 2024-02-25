using System.Security.Cryptography;
using System.Text;
using Dapper;
using Roblox.Dto;
using Roblox.Dto.Avatar;
using Roblox.Models.Assets;
using Roblox.Models.Avatar;
using Roblox.Services.DbModels;
using Roblox.Exceptions.Services.Users;
using Roblox.Rendering;
using Roblox.Services.Exceptions;
using Type = Roblox.Models.Assets.Type;

namespace Roblox.Services;

public class AvatarService : ServiceBase, IService
{
    public async Task<IEnumerable<long>> GetWornAssets(long userId)
    {
        // useless inner join is intentional:
        // it's so that we filter out items the user no longer owns.
        return (await db.QueryAsync<AssetId>(
            "SELECT distinct(ua.asset_id) as assetId FROM user_avatar_asset av INNER JOIN user_asset ua ON ua.user_id = av.user_id AND ua.asset_id = av.asset_id WHERE av.user_id = :user_id", new
            {
                user_id = userId,
            })).Select(c => c.assetId);
    }

    public async Task<bool> IsUserAvatar18Plus(long userId)
    {
        var result = await db.QuerySingleOrDefaultAsync<Dto.Total>
        ("SELECT count(*) AS total FROM user_avatar_asset INNER JOIN asset ON asset.id = user_avatar_asset.asset_id WHERE asset.is_18_plus AND user_avatar_asset.user_id = :user_id",
            new
            {
                user_id = userId,
            });
        return result.total != 0;
    }

    public async Task<IEnumerable<long>> GetRecentItems(long userId)
    {
        var result =
            await db.QueryAsync(
                "SELECT distinct asset_id, max(id) FROM user_asset WHERE user_id = :user_id GROUP BY asset_id ORDER BY max(id) DESC, asset_id LIMIT 50",
                new
                {
                    user_id = userId,
                });
        return result.Select(c => (long) c.asset_id);
    }

    public async Task<AvatarWithColors> GetAvatar(long userId)
    {
        var existingAvatar = await db.QuerySingleOrDefaultAsync<DatabaseAvatarWithImages>(
            "SELECT head_color_id, torso_color_id, left_arm_color_id,right_arm_color_id,left_leg_color_id,right_leg_color_id,thumbnail_url,headshot_thumbnail_url FROM user_avatar WHERE user_id = :user_id",
            new
            {
                user_id = userId,
            });
        return new AvatarWithColors()
        {
            headColorId = existingAvatar.head_color_id,
            torsoColorId = existingAvatar.torso_color_id,
            rightArmColorId = existingAvatar.right_arm_color_id,
            leftArmColorId = existingAvatar.left_arm_color_id,
            rightLegColorId = existingAvatar.right_leg_color_id,
            leftLegColorId = existingAvatar.left_leg_color_id,
            thumbnailUrl = existingAvatar.thumbnail_url,
            headshotUrl = existingAvatar.headshot_thumbnail_url,
        };
    }

    public async Task<ColorEntry> GetAvatarColors(long userId)
    {
        var existingAvatar = await db.QuerySingleOrDefaultAsync<DatabaseAvatar>(
            "SELECT head_color_id, torso_color_id, left_arm_color_id,right_arm_color_id,left_leg_color_id,right_leg_color_id FROM user_avatar WHERE user_id = :user_id",
            new
            {
                user_id = userId,
            });
        return new ColorEntry()
        {
            headColorId = existingAvatar.head_color_id,
            torsoColorId = existingAvatar.torso_color_id,
            rightArmColorId = existingAvatar.right_arm_color_id,
            leftArmColorId = existingAvatar.left_arm_color_id,
            rightLegColorId = existingAvatar.right_leg_color_id,
            leftLegColorId = existingAvatar.left_leg_color_id,
        };
    }

    private readonly Models.Assets.Type[] _wearableAssetTypes = new[]
    {
        Type.Shirt,
        Type.Pants,
        Type.TeeShirt,
        
        Type.Face,
        Type.Hat,
        Type.FrontAccessory,
        Type.BackAccessory,
        Type.WaistAccessory,
        Type.HairAccessory,
        Type.NeckAccessory,
        Type.ShoulderAccessory,
        Type.FaceAccessory,
        
        Type.LeftArm,
        Type.RightArm,
        Type.LeftLeg,
        Type.RightLeg,
        Type.Torso,
        Type.Head,
        
        Type.Gear,
    };
    
    /// <summary>
    /// Filter the dirtyAssetIds. This will remove moderated/pending items, items the user doesn't own, invalid items, etc.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dirtyAssetIds"></param>
    /// <returns></returns>
    public async Task<IEnumerable<long>> FilterAssetsForRender(long userId, IEnumerable<long> dirtyAssetIds)
    {
        var assetIds = dirtyAssetIds.ToList();
        if (assetIds.Count != 0)
        {
            using var assets = ServiceProvider.GetOrCreate<AssetsService>();
            // Get the moderation status for each item
            var moderationStatus = (await db.QueryAsync<AssetModerationEntry>(
                "SELECT moderation_status as moderationStatus, id as assetId, asset_type as assetType FROM asset WHERE id = ANY(:ids)", new
                {
                    ids = assetIds,
                })).ToList();
            // Duplicate the list so we can mutate it
            var safeModList = moderationStatus.ToList();
            // Add package contents, if required
            foreach (var item in moderationStatus)
            {
                if (item.assetType == Type.Package)
                {
                    var packageAssetIds = await assets.GetPackageAssets(item.assetId);
                    var otherModStatus = (await db.QueryAsync<AssetModerationEntry>(
                        "SELECT moderation_status as moderationStatus, id as assetId, asset_type as assetType FROM asset WHERE id = ANY(:ids)", new
                        {
                            ids = packageAssetIds.ToList(),
                        })).ToList();
                    foreach (var nestedAsset in otherModStatus)
                    {
                        safeModList.Add(nestedAsset);
                        assetIds.Add(nestedAsset.assetId);
                    }
                }
            }
            // Filter items by moderation status - we only want to render ReviewApproved
            assetIds = assetIds.Where(c =>
            {
                var hasEntry = safeModList.Find(v => v.assetId == c);
                if (hasEntry == null) return false;
                if (!_wearableAssetTypes.Contains(hasEntry.assetType)) return false;
                if (hasEntry.moderationStatus != ModerationStatus.ReviewApproved) return false;
                return true;
            }).ToList();
            // Finally, confirm user actually owns each assetId
            // goodAssetIds is a list of assets the user owns
            var goodAssetIds = new List<long>();
            foreach (var id in assetIds)
            {
                var ownResult = await db.QuerySingleOrDefaultAsync<UserAssetEntry>(
                    "SELECT asset_id as assetId, user_id as userId from user_asset WHERE user_id = :user_id AND asset_id = :asset_id LIMIT 1",
                    new
                    {
                        user_id = userId,
                        asset_id = id,
                    });
                if (ownResult != null)
                {
                    goodAssetIds.Add(id);
                }
            }

            assetIds = goodAssetIds;
        }

        return assetIds;
    }

    public string GetAvatarHash(ColorEntry colors, IEnumerable<long> assetVersionIds)
    {
        var assets = assetVersionIds.Distinct().ToList();
        assets.Sort((a, b) => a > b ? 1 : a == b ? 0 : -1);
        var str =
            $"avatar-hash-1.4:{string.Join(",", assets)}:{colors.headColorId},{colors.torsoColorId},{colors.leftArmColorId},{colors.rightArmColorId},{colors.leftLegColorId},{colors.rightLegColorId}";
        var hasher = SHA256.Create();
        var bits = hasher.ComputeHash(Encoding.UTF8.GetBytes(str));
        return Convert.ToHexString(bits).ToLower();
    }

    private async Task<IEnumerable<long>> MultiGetAssetVersionsFromAssetIds(IEnumerable<long> assetIds)
    {
        // todo: make this more efficient :(
        var ids = new List<long>();
        using var assets = ServiceProvider.GetOrCreate<AssetsService>(this);
        foreach (var id in assetIds.Distinct())
        {
            var latest = await assets.GetLatestAssetVersion(id);
            ids.Add(latest.assetVersionId);
        }
        return ids.Distinct();
    }

    /// <summary>
    /// Update the userId's avatar. Returns a hash. This does not render or validate anything.
    /// </summary>
    public async Task<string> UpdateUserAvatar(long userId, ColorEntry colors, IEnumerable<long> assetIds)
    {
        var idsList = assetIds.ToList();
        return await InTransaction(async (trx) =>
        {
            await UpdateAsync("user_avatar", "user_id", userId, new
            {
                head_color_id = colors.headColorId,
                torso_color_id = colors.torsoColorId,
                right_arm_color_id = colors.rightArmColorId,
                left_arm_color_id = colors.leftArmColorId,
                right_leg_color_id = colors.rightLegColorId,
                left_leg_color_id = colors.leftLegColorId,
            });
            await db.ExecuteAsync("DELETE FROM user_avatar_asset WHERE user_id = :user_id", new
            {
                user_id = userId,
            });
            foreach (var item in idsList)
            {
                await db.ExecuteAsync("INSERT INTO user_avatar_asset (user_id, asset_id) VALUES (:user_id, :asset_id)",
                    new
                    {
                        user_id = userId,
                        asset_id = item,
                    });
            }

            var assetVersions = await MultiGetAssetVersionsFromAssetIds(idsList);
            return GetAvatarHash(colors, assetVersions);
        });
    }

    public async Task UpdateUserAvatarImages(long userId, string? headshotImage, string? thumbnailImage)
    {
        await db.ExecuteAsync(
            "UPDATE user_avatar SET thumbnail_url = :thumbnail_url, headshot_thumbnail_url = :headshot_url WHERE user_id = :user_id",
            new
            {
                user_id = userId,
                thumbnail_url = thumbnailImage,
                headshot_url = headshotImage,
            });
    }

    public async Task<IEnumerable<OutfitEntry>> GetUserOutfits(long userId, int limit, int offset)
    {
        return await db.QueryAsync<OutfitEntry>(
            "SELECT id, name, created_at as created FROM user_outfit WHERE user_id = :user_id ORDER BY id DESC LIMIT :limit OFFSET :offset",
            new
            {
                user_id = userId,
                limit = limit,
                offset = offset,
            });
    }

    public async Task<OutfitExtendedDetails> GetOutfitById(long outfitId)
    {
        var result = await db.QuerySingleOrDefaultAsync<OutfitAvatar>(
            "SELECT head_color_id as headColorId, torso_color_id as torsoColorId, left_arm_color_id as leftArmColorId, right_arm_color_id as rightArmColorId, left_leg_color_id as leftLegColorId, right_leg_color_id as rightLegColorId, user_id as userId FROM user_outfit WHERE id = :id",
            new
            {
                id = outfitId,
            });
        var assets =
            (await db.QueryAsync<AssetId>(
                "SELECT asset_id as assetId FROM user_outfit_asset WHERE outfit_id = :outfit_id",
                new {outfit_id = outfitId})).Select(c => c.assetId);

        return new OutfitExtendedDetails()
        {
            assetIds = assets,
            details = result,
        };
    }

    public async Task CreateOutfit(long userId, string name, string? thumbnailUrl, string? headshotUrl,
        OutfitExtendedDetails outfitDetails)
    {
        var existingOutfitCount = await db.QuerySingleOrDefaultAsync<Total>(
            "SELECT COUNT(*) as total FROM user_outfit WHERE user_id = :user_id", new
            {
                user_id = userId,
            });
        if (existingOutfitCount.total >= 100)
            throw new TooManyOutfitsException();
        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
            throw new OutfitNameTooShortException();
        if (name.Length > 25)
            throw new OutfitNameTooLongException();
        // image check
        if (string.IsNullOrWhiteSpace(thumbnailUrl) || string.IsNullOrWhiteSpace(headshotUrl))
            throw new NoImageUrlException();

        await InTransaction(async (trx) =>
        {
            var id = await InsertAsync("user_outfit", new
            {
                name = name,
                user_id = userId,
                // colors
                head_color_id = outfitDetails.details.headColorId,
                torso_color_id = outfitDetails.details.torsoColorId,
                left_arm_color_id = outfitDetails.details.leftArmColorId,
                right_arm_color_id = outfitDetails.details.rightArmColorId,
                left_leg_color_id = outfitDetails.details.leftLegColorId,
                right_leg_color_id = outfitDetails.details.rightLegColorId,
                // type
                avatar_type = AvatarType.R6,
                // images
                headshot_thumbnail_url = headshotUrl,
                thumbnail_url = thumbnailUrl,
            });
            foreach (var assetId in outfitDetails.assetIds)
            {
                await InsertAsync("user_outfit_asset", "outfit_id", new
                {
                    outfit_id = id,
                    asset_id = assetId,
                });
            }

            return 0;
        });
    }

    public async Task UpdateOutfit(long outfitId, string name, string? thumbnailUrl, string? headshotUrl,
        OutfitExtendedDetails outfitDetails)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
            throw new OutfitNameTooShortException();
        if (name.Length > 25)
            throw new OutfitNameTooLongException();
        // image check
        if (string.IsNullOrWhiteSpace(thumbnailUrl) || string.IsNullOrWhiteSpace(headshotUrl))
            throw new NoImageUrlException();
        await InTransaction(async (trx) =>
        {
            await UpdateAsync("user_outfit", outfitId, new
            {
                name = name,
                // colors
                head_color_id = outfitDetails.details.headColorId,
                torso_color_id = outfitDetails.details.torsoColorId,
                left_arm_color_id = outfitDetails.details.leftArmColorId,
                right_arm_color_id = outfitDetails.details.rightArmColorId,
                left_leg_color_id = outfitDetails.details.leftLegColorId,
                right_leg_color_id = outfitDetails.details.rightLegColorId,
                // type
                avatar_type = AvatarType.R6,
                // images
                headshot_thumbnail_url = headshotUrl,
                thumbnail_url = thumbnailUrl,
            });
            await db.ExecuteAsync("DELETE FROM user_outfit_asset WHERE outfit_id = :id", new {id = outfitId});
            foreach (var assetId in outfitDetails.assetIds)
            {
                await InsertAsync("user_outfit_asset", "outfit_id", new
                {
                    outfit_id = outfitId,
                    asset_id = assetId,
                });
            }

            return 0;
        });
    }

    public async Task DeleteOutfit(long outfitId)
    {
        await InTransaction(async (t) =>
        {
            await db.ExecuteAsync("DELETE FROM user_outfit WHERE id = :id", new
            {
                id = outfitId,
            });
            await db.ExecuteAsync("DELETE FROM user_outfit_asset WHERE outfit_id = :outfit_id", new
            {
                outfit_id = outfitId,
            });
            return 0;
        });
    }

    private async Task<bool> ConfirmAssetSelectionIsOkForRender(IEnumerable<long> unknownAssetIds)
    {
        var assets = new AssetsService();
        var assetIds = unknownAssetIds.ToList();
        if (assetIds.Count == 0) return true;
        var details = await assets.MultiGetInfoById(assetIds);

        // vars
        var gear = 0;
        var face = 0;
        var shirt = 0;
        var pants = 0;
        var tShirt = 0;
        var accessories = 0;
        var leftArm = 0;
        var rightArm = 0;
        var leftLeg = 0;
        var rightLeg = 0;
        var torso = 0;
        var head = 0;
        var animations = 0;

        foreach (var item in details)
        {
            switch (item.assetType)
            {
                case Models.Assets.Type.TeeShirt:
                    tShirt++;
                    break;
                case Models.Assets.Type.Shirt:
                    shirt++;
                    break;
                case Models.Assets.Type.Pants:
                    pants++;
                    break;
                case Models.Assets.Type.Animation:
                    animations++;
                    break;
                case Models.Assets.Type.Gear:
                    gear++;
                    break;
                case Models.Assets.Type.Face:
                    face++;
                    break;
                case Models.Assets.Type.Hat:
                case Models.Assets.Type.FrontAccessory:
                case Models.Assets.Type.BackAccessory:
                case Models.Assets.Type.HairAccessory:
                case Models.Assets.Type.NeckAccessory:
                case Models.Assets.Type.ShoulderAccessory:
                case Models.Assets.Type.WaistAccessory:
                case Models.Assets.Type.FaceAccessory:
                    accessories++;
                    break;
                case Models.Assets.Type.Head:
                    head++;
                    break;
                case Models.Assets.Type.Torso:
                    torso++;
                    break;
                case Models.Assets.Type.LeftArm:
                    leftArm++;
                    break;
                case Models.Assets.Type.RightArm:
                    rightArm++;
                    break;
                case Models.Assets.Type.LeftLeg:
                    leftLeg++;
                    break;
                case Models.Assets.Type.RightLeg:
                    rightLeg++;
                    break;
                default:
                    throw new Exception("Unexpected asset type: " + item.assetType);
            }
        }

        if (animations > 1) return false;
        if (gear > 1) return false;
        if (tShirt > 1 || shirt > 1 || pants > 1) return false;
        if (face > 1) return false;
        if (accessories > 6) return false;
        if (leftArm > 1 || rightArm > 1 || leftLeg > 1 || rightLeg > 1 || torso > 1 || head > 1) return false;
        return true;
    }

    private bool IsColorValid(int color)
    {
        var allColors = Roblox.Models.Avatar.AvatarMetadata.GetColors();
        foreach (var item in allColors)
        {
            if (item.brickColorId == color)
            {
                return true;
            }
        }

        return false;
    }

    public bool AreColorsOk(ColorEntry colors)
    {
        if (!IsColorValid(colors.headColorId)) return false;
        if (!IsColorValid(colors.torsoColorId)) return false;
        if (!IsColorValid(colors.leftArmColorId)) return false;
        if (!IsColorValid(colors.rightArmColorId)) return false;
        if (!IsColorValid(colors.leftLegColorId)) return false;
        if (!IsColorValid(colors.rightLegColorId)) return false;
        return true;
    }

    public string GetAvatarRedLockKey(long userId)
    {
        return $"update avatar web v1 {userId}";
    }

    public async Task RedrawAvatar(long userId, IEnumerable<long>? newAssetIds = null, ColorEntry? colors = null,
        AvatarType? avatarType = null, bool forceRedraw = false, bool ignoreLock = true)
    {
        // required services
        using var assets = ServiceProvider.GetOrCreate<AssetsService>();

        // params
        avatarType ??= AvatarType.R6;

        await using var redLock =
            await Cache.redLock.CreateLockAsync(GetAvatarRedLockKey(userId), TimeSpan.FromSeconds(5));
        if (!redLock.IsAcquired && !ignoreLock) throw new LockNotAcquiredException();

        var assetIds = newAssetIds?.ToList();

        // If list provided is null, then the caller wants us to grab the items ourselves
        assetIds ??= (await GetWornAssets(userId)).ToList();
        colors ??= await GetAvatarColors(userId);

        if (!AreColorsOk(colors))
            throw new RobloxException(400, 0, "Colors are invalid");


        if (assetIds.Count != 0)
        {
            assetIds = (await FilterAssetsForRender(userId, assetIds)).ToList();
        }

        var assetsOk = await ConfirmAssetSelectionIsOkForRender(assetIds);
        if (!assetsOk)
            throw new RobloxException(400, 0, "One or more assets are invalid");
        // Now, update the avatar. This returns a hash
        var avatarHash = await UpdateUserAvatar(userId, colors, assetIds);
        // Get our image urls
        var thumbnailUrl = $"/images/thumbnails/{avatarHash}_thumbnail.png";
        var headshotUrl = $"/images/thumbnails/{avatarHash}_headshot.png";
        if (!forceRedraw)
        {
            // Check if the hash exists already - If they do, we can skip rendering!
            if (File.Exists(Configuration.PublicDirectory + thumbnailUrl) &&
                File.Exists(Configuration.PublicDirectory + headshotUrl))
            {
                // Since both files exist, we can just update the URL and exit
                await UpdateUserAvatarImages(userId, headshotUrl, thumbnailUrl);
                return;
            }
        }

        // We have to call render library now.
        // Set image urls to null:
        await UpdateUserAvatarImages(userId, null, null);
        // Create request
        var extendedAssetDetails = await assets.MultiGetInfoById(assetIds);
        var request = new Roblox.Rendering.AvatarData()
        {
            userId = userId,
            assets = extendedAssetDetails.Select(c => new AvatarAssetEntry()
            {
                id = c.id,
                assetType = new AvatarAssetTypeEntry()
                {
                    id = (int) c.assetType,
                },
            }),
            bodyColors = new AvatarBodyColors()
            {
                headColorId = colors.headColorId,
                torsoColorId = colors.torsoColorId,
                leftArmColorId = colors.leftArmColorId,
                rightArmColorId = colors.rightArmColorId,
                leftLegColorId = colors.leftLegColorId,
                rightLegColorId = colors.rightLegColorId,
            },
            playerAvatarType = "R6",
        };
        // Sane timeout of 30 minutes. If a render takes longer than this, something's probably broken
        using var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(TimeSpan.FromSeconds(30));
        // Make both requests at once
        var result = await Task.WhenAll(new List<Task<Stream>>()
        {
            CommandHandler.RequestPlayerHeadshot(request, cancellation.Token),
            CommandHandler.RequestPlayerThumbnail(request, cancellation.Token),
        });
        var headshotStream = result[0];
        var thumbnailStream = result[1];
        // Write the files
        await using (var fileStream = File.Create(Configuration.PublicDirectory + headshotUrl))
        {
            headshotStream.Seek(0, SeekOrigin.Begin);
            await headshotStream.CopyToAsync(fileStream);
        }

        await using (var fileStream = File.Create(Configuration.PublicDirectory + thumbnailUrl))
        {
            thumbnailStream.Seek(0, SeekOrigin.Begin);
            await thumbnailStream.CopyToAsync(fileStream);
        }

        // Finally, update the avatar thumbnail
        await UpdateUserAvatarImages(userId, headshotUrl, thumbnailUrl);
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