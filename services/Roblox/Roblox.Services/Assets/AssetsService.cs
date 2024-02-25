using System.Diagnostics;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dapper;
using FFMpegCore;
using Roblox.Dto;
using Roblox.Dto.Assets;
using Roblox.Dto.Users;
using Roblox.Exceptions.Services.Assets;
using Roblox.Libraries;
using Roblox.Logging;
using Roblox.Metrics;
using Roblox.Models.Assets;
using Roblox.Models.Economy;
using Roblox.Models.Groups;
using Roblox.Rendering;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.DbModels;
using Roblox.Services.Exceptions;
using SixLabors.ImageSharp;
using AssetId = Roblox.Dto.Assets.AssetId;
using MultiGetEntry = Roblox.Dto.Assets.MultiGetEntry;
using Type = Roblox.Models.Assets.Type;

namespace Roblox.Services;

public class AssetsService : ServiceBase, IService
{
    public async Task<long> GetAssetIdFromRobloxAssetId(long robloxAssetId)
    {
        var result = await db.QuerySingleOrDefaultAsync<Dto.Assets.AssetId>(
            "SELECT id as assetId FROM asset WHERE roblox_asset_id = :id LIMIT 1", new
            {
                id = robloxAssetId,
            });
        if (result == null) throw new RecordNotFoundException();

        return result.assetId;
    }

    public async Task<Dto.Assets.AssetVersionEntry> GetLatestAssetVersion(long assetId)
    {
        var result = await db.QuerySingleOrDefaultAsync<Dto.Assets.AssetVersionEntry>(
            "SELECT id as assetVersionId, version_number as versionNumber, content_url as contentUrl, content_id as contentId, created_at as createdAt, updated_at as updatedAt, creator_id as creatorId FROM asset_version WHERE asset_id = :id ORDER BY id DESC LIMIT 1",
            new
            {
                id = assetId,
            });
        if (result == null) throw new RecordNotFoundException();
        return result;
    }

    private void ValidateNameAndDescription(string name, string? description)
    {
        // Validation
        if (string.IsNullOrEmpty(name)) throw new AssetNameTooShortException();
        if (name.Length > Models.Assets.Rules.NameMaxLength)
            throw new AssetNameTooLongException();
        if (description is {Length: > Models.Assets.Rules.DescriptionMaxLength})
            throw new AssetDescriptionTooLongException();

        return;
    }

    public async Task<Stream> GetAssetContent(string key)
    {
        if (key.Contains('/', StringComparison.Ordinal))
        {
            Metrics.SecurityMetrics.ReportBadCharacterFoundInAssetContentName(key, "/", "GetAssetContent");
            throw new ArgumentException("GetAssetContent error 1");
        }
        
        var fullPath = Configuration.AssetDirectory + key;
        for (var i = 0; i < 10; i++)
        {
            try
            {
                var file = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, default,
                    FileOptions.Asynchronous);
                return file;
            }
            catch (Exception e) when (e is IOException)
            {
                Writer.Info(LogGroup.AssetDelivery, "GetAssetContent IO exception. Message = {0}\n{1}", e.Message, e.StackTrace);
                if (e.Message.Contains("Could not find file"))
                    throw;
                
                await Task.Delay(TimeSpan.FromMilliseconds(100 * (i+1)));
            }
        }

        throw new Exception("Maximum retry attempts reached for GetAssetContent(" + key + ")");
    }

    private async Task<string> UploadAssetContent(Stream content, string? directory = null,
        string? extension = null)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.UploadContentEnabled);
        directory ??= Configuration.StorageDirectory;

        // validation
        if (!content.CanRead)
        {
            throw new Exception("Invalid stream");
        }

        // Reset
        content.Position = 0;
        var sha256 = SHA256.Create();
        var bin = await sha256.ComputeHashAsync(content);
        // Reset again
        content.Position = 0;
        // Hash without extension
        var plainHash = Convert.ToHexString(bin).ToLowerInvariant();
        var hash = plainHash;
        if (!string.IsNullOrEmpty(extension))
        {
            hash += "." + extension;
        }

        var outPath = directory + hash;
        // We got our hash now. Check if the file already exists.
        if (File.Exists(outPath))
        {
            // File already exists!
            return plainHash;
        }

        // Insert the file
        await using var file = File.Create(outPath);
        content.Seek(0, SeekOrigin.Begin);
        await content.CopyToAsync(file);
        // Done
        return plainHash;
    }

    public Task DeleteAssetContent(string key, string? directory = null)
    {
        if (key.Contains('/', StringComparison.Ordinal))
        {
            Metrics.SecurityMetrics.ReportBadCharacterFoundInAssetContentName(key, "/", "DeleteAssetContent");
            throw new ArgumentException("DeleteAssetContent error 1");
        }
        
        directory ??= Configuration.AssetDirectory;

        var fullPath = directory + key;
        while (true)
        {
            try
            {
                File.Delete(fullPath);
                break;
            }
            catch (FileNotFoundException e)
            {
                Metrics.SecurityMetrics.ReportErrorDeletingAssetContent(key, e.StackTrace ?? new Exception().StackTrace ?? "NotGenerated", e.Message);
                break; // should report but don't throw
            }
            catch (Exception e)
            {
                // TODO: what about when a file is being used by something? should be keep retrying?
                Metrics.SecurityMetrics.ReportErrorDeletingAssetContent(key, e.StackTrace ?? new Exception().StackTrace ?? "NotGenerated", e.Message);
                throw;
            }
        }

        return Task.CompletedTask;
    }

    public async Task InsertOrReplaceThumbnail(long assetId, long assetVersionId, string newThumbnailKey,
        Models.Assets.ModerationStatus moderationStatus)
    {
        await InTransaction(async (tr) =>
        {
            await db.ExecuteAsync("DELETE FROM asset_thumbnail WHERE asset_id = :asset_id", new
            {
                asset_id = assetId,
            });
            await InsertAsync("asset_thumbnail", "asset_id", new
            {
                asset_id = assetId,
                content_url = newThumbnailKey,
                moderation_status = moderationStatus,
                asset_version_id = assetVersionId,
            });
            return 0;
        });
    }

    public async Task InsertOrReplaceIcon(long assetId, string newThumbnailKey,
        Models.Assets.ModerationStatus moderationStatus)
    {
        await InTransaction(async (tr) =>
        {
            await db.ExecuteAsync("DELETE FROM asset_icon WHERE asset_id = :asset_id", new
            {
                asset_id = assetId,
            });
            await InsertAsync("asset_icon", new
            {
                asset_id = assetId,
                content_url = newThumbnailKey,
                moderation_status = moderationStatus,
            });
            return 0;
        });
    }

    internal class AssetValidationResponse
    {
        public bool isValid { get; set; }
    }

    private static HttpClient assetValidationClient { get; } = new();

    public async Task<bool> ValidateAssetFile(Stream file, Models.Assets.Type assetType)
    {
        Writer.Info(LogGroup.AssetValidation, "validating asset. type = {0}", assetType);
        try
        {
            var s = new StreamContent(file);
            s.Headers.Add("robloxAuthorization", Configuration.AssetValidationServiceAuthorization);
            var url = Configuration.AssetValidationServiceUrl + "/api/v1/validate-item";
            if (assetType == Type.Place)
            {
                url = Configuration.AssetValidationServiceUrl + "/api/v1/validate-place";
            }
            var ok = await assetValidationClient.PostAsync(
                url, s);
            if (!ok.IsSuccessStatusCode)
            {
                throw new Exception("Got failure response from assetValidationService. Code = " + ok.StatusCode);
            }

            var result = JsonSerializer.Deserialize<AssetValidationResponse>(await ok.Content.ReadAsStringAsync());
            return result is {isValid: true};
        }
        catch (Exception e)
        {
            Writer.Info(LogGroup.AssetValidation, "ValidateAssetFile caught exception. message = {0}\n{1}", e.Message, e.StackTrace);
        }

        return false;
    }

    public async Task<Imager?> ValidateImage(Stream content)
    {
        var imageData = await Imager.ReadAsync(content);

        if (imageData == null) return null;
        if (imageData.width <= 0 || imageData.height <= 0)
            return null;

        if (imageData.imageFormat != ImagerFormat.PNG && imageData.imageFormat != ImagerFormat.JPEG)
            return null;

        return imageData;
    }

    public async Task<Imager?> ValidateClothing(Stream content, Models.Assets.Type type)
    {
        Imager? img = null;
        try
        {
            img = await Imager.ReadAsync(content);
        }
        catch (Exception e) when (e is InvalidImageException or UnsupportedImageFormatException)
        {
            AssetMetrics.ReportInvalidClothingFileUploadAttempt(e.Message + "\n" + e.StackTrace);
            return null;
        }

        if (img == null) return null;

        if (img.imageFormat != ImagerFormat.JPEG && img.imageFormat != ImagerFormat.PNG)
        {
            AssetMetrics.ReportInvalidClothingImageFormatUploadAttempt(img.imageFormat.ToString());
            return null;
        }

        if (type is Models.Assets.Type.Pants or Models.Assets.Type.Shirt)
        {
            // Must be these exact dimensions
            if (img.width == 585 && img.height == 559)
                return img;
        }

        if (type == Models.Assets.Type.TeeShirt)
            return img;

        return null;
    }
    
    private static AsyncLimit audioConversionLimit { get; } = new("AudioConversionLimit", 5);

    public async Task<Stream> GetAudioContentAsWav(long assetId, string contentUrl)
    {
        var fileName = Configuration.StorageDirectory + "/" + contentUrl + "_wav_cache_v1.wav";
        if (File.Exists(fileName))
        {
            Writer.Info(LogGroup.AudioConversion, $"use cache for {assetId} - {contentUrl}");
            return new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, default,
                FileOptions.Asynchronous);
        }
        await using var limit = await audioConversionLimit.CreateAsync(TimeSpan.FromSeconds(10));
        // files used because I couldn't get streaming/pipes working. hopefully temporary anyway
        var tempFile = Path.GetTempFileName();

        try
        {
            var assetContent = await GetAssetContent(contentUrl);
            assetContent.Position = 0;
            Writer.Info(LogGroup.AudioConversion, "converting {0} to wav", assetId);

            await using (var fs = File.OpenWrite(tempFile))
            {
                assetContent.Seek(0, SeekOrigin.Begin);
                await assetContent.CopyToAsync(fs);
            }

            await FFMpegArguments
                .FromFileInput(tempFile)
                .OutputToFile(fileName, true, options => options.WithAudioCodec("pcm_s16le"))
                .ProcessAsynchronously();

            return new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, default,
                FileOptions.Asynchronous);
        }
        catch (Exception e)
        {
            try
            {
                File.Delete(fileName);
            }
            catch (Exception deletionErr)
            {
                Writer.Info(LogGroup.AudioConversion, $"attempt to delete {fileName} after failed conversion but error: {deletionErr.Message}\n{deletionErr.StackTrace}");
            }
            Writer.Info(LogGroup.AudioConversion, "error converting audio to wav for game {0}\n{1}", e.Message, e.StackTrace);
            throw;
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    public async Task<bool> Is18Plus(long assetId)
    {
        var result = await db.QuerySingleOrDefaultAsync<Dto.Assets.IsAsset18Plus>(
            "SELECT is_18_plus AS is18Plus FROM asset WHERE id = :id",
            new
            {
                id = assetId,
            });
        return result.is18Plus;
    }

    private static long maxAudioFileSizeBytes = 20447232;

    public async Task<AudioValidation> IsAudioValid(Stream content)
    {
        if (content.Length > maxAudioFileSizeBytes) return AudioValidation.FileTooLarge;
        if (content.Length == 0) return AudioValidation.EmptyStream;
        content.Position = 0;
        IMediaAnalysis mediaInfo;
        // streams return an empty duration, so we have to write to disk and then read that...
        // https://github.com/rosenbjerg/FFMpegCore/issues/130#issuecomment-739572946
        var tempFile = Path.GetTempFileName();
        try
        {
            await using (var fs = File.OpenWrite(tempFile))
            {
                content.Seek(0, SeekOrigin.Begin);
                await content.CopyToAsync(fs);
            }

            mediaInfo = await FFProbe.AnalyseAsync(tempFile);
        }
        catch (Exception e)
        {
            Console.WriteLine("[error] error validating audio: {0}\n{1}", e.Message, e.StackTrace);
            return AudioValidation.UnsupportedFormat;
        }
        finally
        {
            File.Delete(tempFile);
        }

        if (mediaInfo.Duration > TimeSpan.FromMinutes(7)) return AudioValidation.TooLong;
        if (mediaInfo.Duration < TimeSpan.FromMilliseconds(10))
            return
                AudioValidation
                    .TooShort; // If duration is 0, FFProbe probably messed up, and we don't want to risk having users upload infinite duration files
        var formatDetails = mediaInfo.Format;
        // our game engine currently supports mp3 and ogg.
        if (formatDetails.FormatName is "mp3" or "ogg")
        {
            // OK
            return AudioValidation.Ok;
        }

        return AudioValidation.UnsupportedFormat;
    }

    #region RenderMethods

    private async Task CreateAssetTextureThumbnail(long assetId, Models.Assets.Type assetType, CancellationToken? cancellationToken = null)
    {
        var latestVersion = await GetLatestAssetVersion(assetId);
        var response = await Rendering.CommandHandler.RequestTextureThumbnail(assetId, (int) assetType, cancellationToken);
        var key = await UploadAssetContent(response, Configuration.ThumbnailsDirectory, "png");
        await InsertOrReplaceThumbnail(assetId, latestVersion.assetVersionId, key, ModerationStatus.ReviewApproved);
    }
    
    private async Task CreatePackageThumbnail(long assetId, CancellationToken? cancellationToken = null)
    {
        var latestVersion = await GetLatestAssetVersion(assetId);
        var assets = await MultiGetAssetDeveloperDetails(await GetPackageAssets(assetId));
        var renderAssets = assets.Select(c => new AvatarAssetEntry()
        {
            id = c.assetId,
            assetType = new AvatarAssetTypeEntry()
            {
                id = c.typeId,
            }
        }).ToList();
        renderAssets.Add(new AvatarAssetEntry()
        {
            id = Configuration.PackageShirtAssetId,
            assetType = new AvatarAssetTypeEntry()
            {
                id = (int)Type.Shirt,
            },
        });
        renderAssets.Add(new AvatarAssetEntry()
        {
            id = Configuration.PackagePantsAssetId,
            assetType = new AvatarAssetTypeEntry()
            {
                id = (int)Type.Pants,
            },
        });
        var response = await Rendering.CommandHandler.RequestPlayerThumbnail(new()
        {
            userId = assetId,
            playerAvatarType = "R6",
            assets = renderAssets,
            bodyColors = new AvatarBodyColors()
            {
                headColorId = 1001,
                torsoColorId = 1001,
                leftArmColorId = 1001,
                rightArmColorId = 1001,
                leftLegColorId = 1001,
                rightLegColorId = 1001,
            },
        }, cancellationToken);
        var key = await UploadAssetContent(response, Configuration.ThumbnailsDirectory, "png");
        await InsertOrReplaceThumbnail(assetId, latestVersion.assetVersionId, key,
            ModerationStatus.AwaitingApproval);
    }

    private async Task CreateAssetThumbnail(long assetId, CancellationToken? cancellationToken = null)
    {
        var latestVersion = await GetLatestAssetVersion(assetId);
        var response = await Rendering.CommandHandler.RequestAssetThumbnail(assetId, cancellationToken);
        var key = await UploadAssetContent(response, Configuration.ThumbnailsDirectory, "png");
        await InsertOrReplaceThumbnail(assetId, latestVersion.assetVersionId, key, ModerationStatus.ReviewApproved);
    }

    private async Task CreateMeshThumbnail(long assetId, CancellationToken? cancellationToken = null)
    {
        var latestVersion = await GetLatestAssetVersion(assetId);
        var response = await Rendering.CommandHandler.RequestAssetMesh(assetId, cancellationToken);
        var key = await UploadAssetContent(response, Configuration.ThumbnailsDirectory, "png");
        await InsertOrReplaceThumbnail(assetId, latestVersion.assetVersionId, key, ModerationStatus.ReviewApproved);
    }

    private async Task CreateHeadThumbnail(long assetId, CancellationToken? cancellationToken = null)
    {
        var latestVersion = await GetLatestAssetVersion(assetId);
        var response = await Rendering.CommandHandler.RequestHeadThumbnail(assetId, cancellationToken);
        var key = await UploadAssetContent(response, Configuration.ThumbnailsDirectory, "png");
        await InsertOrReplaceThumbnail(assetId, latestVersion.assetVersionId, key, ModerationStatus.ReviewApproved);
    }


    private async Task CreateGameThumbnail(long assetId, CancellationToken? cancellationToken = null)
    {
        var latestVersion = await GetLatestAssetVersion(assetId);
        var response = await Rendering.CommandHandler.RequestAssetGame(assetId, 640, 360, cancellationToken);
        var key = await UploadAssetContent(response, Configuration.ThumbnailsDirectory, "png");
        await InsertOrReplaceThumbnail(assetId, latestVersion.assetVersionId, key,
            ModerationStatus.AwaitingApproval);
    }

    /// <summary>
    /// Convert a Roblox place to an enconomy simulator place. Returns the stream of the converted place.
    /// </summary>
    /// <param name="rawPlaceStream">Stream of the raw place file</param>
    /// <param name="cancellationToken">The cancellationToken</param>
    /// <returns></returns>
    public async Task<Stream> ConvertRobloxPlace(Stream rawPlaceStream, CancellationToken? cancellationToken = null)
    {
        byte[] bytes;
        await using (var memoryStream = new MemoryStream())
        {
            await rawPlaceStream.CopyToAsync(memoryStream);
            bytes = memoryStream.ToArray();
        }

        var base64Request = Convert.ToBase64String(bytes);
        return await CommandHandler.RequestPlaceConversion(base64Request, cancellationToken);
    }

    /// <summary>
    /// Convert a hat/gear/accessory/whatever to a hat that works on the website. Returns a stream of the new item.
    /// </summary>
    /// <param name="rawHatStream">The stream of the asset content to convert</param>
    /// <param name="cancellationToken">The cancellationToken</param>
    /// <returns></returns>
    public async Task<Stream> ConvertHat(Stream rawHatStream, CancellationToken? cancellationToken = null)
    {
        byte[] bytes;
        await using (var memoryStream = new MemoryStream())
        {
            await rawHatStream.CopyToAsync(memoryStream);
            bytes = memoryStream.ToArray();
        }

        var base64Request = Convert.ToBase64String(bytes);
        return await CommandHandler.RequestHatConversion(base64Request, cancellationToken);
    }

    private async Task CreateGameIcon(long assetId, Stream? thumbnailToUse = null, CancellationToken? cancellationToken = null)
    {
        if (thumbnailToUse == null)
        {
            Writer.Info(LogGroup.GameIconRender, "start game icon render. placeId={0}", assetId);
            thumbnailToUse = await Rendering.CommandHandler.RequestAssetGame(assetId, 420, 420, cancellationToken);
            Writer.Info(LogGroup.GameIconRender, "game icon render over. placeId={0}", assetId);
        }

        var key = await UploadAssetContent(thumbnailToUse, Configuration.ThumbnailsDirectory, "png");
        await InsertOrReplaceIcon(assetId, key, ModerationStatus.AwaitingApproval);
    }

    private async Task CreateTeeShirtThumbnail(long assetId, CancellationToken? cancellationToken = null)
    {
        var latestVersion = await GetLatestAssetVersion(assetId);
        var thumbnailToUse = await Rendering.CommandHandler.RequestAssetTeeShirt(assetId, latestVersion.contentId, cancellationToken);
        var key = await UploadAssetContent(thumbnailToUse, Configuration.ThumbnailsDirectory, "png");
        await InsertOrReplaceThumbnail(assetId, latestVersion.assetVersionId, key,
            ModerationStatus.AwaitingApproval);
    }

    private async Task CreateRawImageThumbnail(long assetId, CancellationToken? cancellationToken = null)
    {
        var latestVersion = await GetLatestAssetVersion(assetId);
        if (latestVersion.contentUrl == null)
            throw new Exception("Latest asset version has no contentUrl");
        var thumbnailToUse = await GetAssetContent(latestVersion.contentUrl);
        var key = await UploadAssetContent(thumbnailToUse, Configuration.ThumbnailsDirectory, "png");
        await InsertOrReplaceThumbnail(assetId, latestVersion.assetVersionId, key,
            ModerationStatus.AwaitingApproval);
    }

    private async Task CreateClothingThumbnail(long assetId, Models.Assets.Type assetType, CancellationToken? cancellationToken = null)
    {
        var latestVersion = await GetLatestAssetVersion(assetId);
        var response = await Rendering.CommandHandler.RequestPlayerThumbnail(new()
        {
            userId = assetId,
            playerAvatarType = "R6",
            assets = new List<AvatarAssetEntry>()
            {
                new AvatarAssetEntry()
                {
                    id = assetId,
                    assetType = new AvatarAssetTypeEntry()
                    {
                        id = (int) assetType,
                    }
                }
            },
            bodyColors = new AvatarBodyColors()
            {
                headColorId = 1001,
                torsoColorId = 1001,
                leftArmColorId = 1001,
                rightArmColorId = 1001,
                leftLegColorId = 1001,
                rightLegColorId = 1001,
            },
        }, cancellationToken);
        var key = await UploadAssetContent(response, Configuration.ThumbnailsDirectory, "png");
        await InsertOrReplaceThumbnail(assetId, latestVersion.assetVersionId, key,
            ModerationStatus.AwaitingApproval);
    }

    private async Task CreateBodyPartThumbnail(long assetId, Models.Assets.Type assetType, CancellationToken? cancellationToken = null)
    {
        // get the clothing id, which is mainly why we need a whole serperate function
        // TODO: Maybe make this one function in CreateClothingThumbnail?

        var clothingId = (long) 0; // this has to be here so shit doesn't complain
        var modelThing = Models.Assets.Type.Torso; // TODO: Do we really need this?
        // start setting up the values

        if (Models.Assets.Type.Torso == assetType) {
            modelThing = Models.Assets.Type.Torso;
            clothingId = Roblox.Configuration.PackageTorsoAssetId;
        }

        if (Models.Assets.Type.LeftArm == assetType) {
            modelThing = Models.Assets.Type.LeftArm;
            clothingId = Roblox.Configuration.PackageLeftArmAssetId;
        }

        if (Models.Assets.Type.RightArm == assetType) {
            modelThing = Models.Assets.Type.RightArm;
            clothingId = Roblox.Configuration.PackageRightArmAssetId;
        }

        if (Models.Assets.Type.LeftLeg == assetType) {
            modelThing = Models.Assets.Type.LeftLeg;
            clothingId = Roblox.Configuration.PackageLeftLegAssetId;
        }

        if (Models.Assets.Type.RightLeg == assetType) {
            modelThing = Models.Assets.Type.RightLeg;
            clothingId = Roblox.Configuration.PackageRightLegAssetId;
        }

        var latestVersion = await GetLatestAssetVersion(assetId);
        var response = await Rendering.CommandHandler.RequestPlayerThumbnail(new()
        {
            userId = assetId,
            playerAvatarType = "R6",
            assets = new List<AvatarAssetEntry>()
            {
                new AvatarAssetEntry()
                {
                    id = assetId,
                    assetType = new AvatarAssetTypeEntry()
                    {
                        id = (int) assetType,
                    }
                },
                new AvatarAssetEntry()
                {
                    id = clothingId,
                    assetType = new AvatarAssetTypeEntry()
                    {
                        id = (int) modelThing,
                    }
                }
            },
            bodyColors = new AvatarBodyColors()
            {
                headColorId = 1001,
                torsoColorId = 1001,
                leftArmColorId = 1001,
                rightArmColorId = 1001,
                leftLegColorId = 1001,
                rightLegColorId = 1001,
            },
        }, cancellationToken);
        var key = await UploadAssetContent(response, Configuration.ThumbnailsDirectory, "png");
        await InsertOrReplaceThumbnail(assetId, latestVersion.assetVersionId, key,
            ModerationStatus.AwaitingApproval);
    }

    #endregion

    /// <summary>
    /// Render asset and wait for it to finish
    /// </summary>
    /// <param name="assetId"></param>
    /// <param name="assetType"></param>
    /// <param name="cancellationToken">The CancellationToken</param>
    /// <exception cref="Exception"></exception>
    public async Task RenderAssetAsync(long assetId, Models.Assets.Type assetType, CancellationToken? cancellationToken = null)
    {
        List<Task> thumbRequests = new();
        switch (assetType)
        {
            case Models.Assets.Type.Image:
            case Models.Assets.Type.Decal:
            case Models.Assets.Type.Face:
                thumbRequests.Add(CreateAssetTextureThumbnail(assetId, assetType, cancellationToken));
                break;
            // clothing
            case Models.Assets.Type.Shirt:
            case Models.Assets.Type.Pants:
                 thumbRequests.Add(CreateClothingThumbnail(assetId, assetType, cancellationToken));
                 break;
            // package stuff
            case Type.Head:
                 thumbRequests.Add(CreateHeadThumbnail(assetId, cancellationToken));
                 break;
            case Type.Torso:
            case Type.LeftArm: 
            case Type.RightArm:
            case Type.LeftLeg:
            case Type.RightLeg:
                thumbRequests.Add(CreateBodyPartThumbnail(assetId, assetType, cancellationToken));
                break;
            case Models.Assets.Type.Package:
                thumbRequests.Add(CreatePackageThumbnail(assetId, cancellationToken));
                break;
            case Models.Assets.Type.TeeShirt:
                thumbRequests.Add(CreateTeeShirtThumbnail(assetId, cancellationToken));
                break;
            // items without custom icons
            case Models.Assets.Type.Animation:
            case Models.Assets.Type.Audio:
            case Models.Assets.Type.ClimbAnimation:
            case Models.Assets.Type.DeathAnimation:
            case Models.Assets.Type.FallAnimation:
            case Models.Assets.Type.Lua:
            case Models.Assets.Type.IdleAnimation:
            case Models.Assets.Type.WalkAnimation:
            case Models.Assets.Type.RunAnimation:
            case Models.Assets.Type.JumpAnimation:
            case Models.Assets.Type.PoseAnimation:
            case Models.Assets.Type.SwimAnimation:
            case Models.Assets.Type.Plugin:
            case Models.Assets.Type.SolidModel:
                break;
            case Models.Assets.Type.Badge:
            case Models.Assets.Type.GamePass:
                // todo: create icon with same method as tee shirt render (no bg), but crop result to circle
                throw new Exception("NotImplemented");
            case Models.Assets.Type.Place:
                thumbRequests.Add(CreateGameThumbnail(assetId, cancellationToken));
                thumbRequests.Add(CreateGameIcon(assetId, default, cancellationToken));
                break;
            case Models.Assets.Type.Mesh:
            case Models.Assets.Type.MeshPart:
                thumbRequests.Add(CreateMeshThumbnail(assetId, cancellationToken));
                break;
            case Type.Hat:
            case Type.Gear:
            case Type.HairAccessory:
            case Type.NeckAccessory:
            case Type.ShoulderAccessory:
            case Type.BackAccessory:
            case Type.FrontAccessory:
            case Type.FaceAccessory:
            case Type.WaistAccessory:
                thumbRequests.Add(CreateAssetThumbnail(assetId, cancellationToken));
                break;
            default:
                Writer.Info(LogGroup.AssetRender, "Unexpected assetType {0}", assetType);
                throw new Exception("Unexpected assetType: " + assetType);
        }

        if (thumbRequests.Count == 0)
            return;

        try
        {
            Console.WriteLine("Start multi render");
            //await Task.WhenAll(thumbRequests);
            Console.WriteLine("End multi render");
        }
        catch (System.Exception e)
        {
            Console.WriteLine("[error] Render failed for {0}:{1}: {2}", assetId, assetType, e.Message);
        }
    }

    public void RenderAsset(long assetId, Models.Assets.Type assetType)
    {
        Task.Run(async () => { await RenderAssetAsync(assetId, assetType); });
    }

    public async Task<CreateResponse> CreateAssetVersion(long assetId, long creatorUserId, long contentId)
    {
        var latest = await GetLatestAssetVersion(assetId);
        var created = DateTime.UtcNow;

        var id = await InsertAsync("asset_version", new
        {
            asset_id = assetId,
            version_number = latest.versionNumber + 1,
            creator_id = creatorUserId,
            created_at = created,
            updated_at = created,
            content_id = contentId,
        });
        
        await UpdateAsset(assetId);

        return new()
        {
            assetId = assetId,
            assetVersionId = id,
        };
    }

    private async Task UpdateAsset(long assetId)
    {
        await db.ExecuteAsync("UPDATE asset SET updated_at = now() WHERE id = :id", new
        {
            id = assetId,
        });
    }

    public async Task<CreateResponse> CreateAssetVersion(long assetId, long creatorUserId, Stream assetContent)
    {
        var latest = await GetLatestAssetVersion(assetId);
        var fileId = await UploadAssetContent(assetContent, Configuration.AssetDirectory);
        var created = DateTime.UtcNow;

        var id = await InsertAsync("asset_version", new
        {
            asset_id = assetId,
            version_number = latest.versionNumber + 1,
            creator_id = creatorUserId,
            created_at = created,
            updated_at = created,
            content_url = fileId,
        });

        await UpdateAsset(assetId);

        return new()
        {
            assetId = assetId,
            assetVersionId = id,
        };
    }

    private static readonly Models.Assets.Type[] TypesToGrantOnCreation = new[]
    {
        Type.Hat,
        Type.HairAccessory,
        Type.FrontAccessory,
        Type.BackAccessory,
        Type.WaistAccessory,
        Type.NeckAccessory,
        Type.Gear,
        Type.Face,
        Type.ShoulderAccessory,
        Type.FaceAccessory,
        Type.Head,
        Type.RightArm,
        Type.LeftArm,
        Type.Torso,
        Type.RightLeg,
        Type.LeftLeg,
        Type.Package,
    };

    public async Task<Dto.Assets.CreateResponse> CreateAsset(string name, string? description, long creatorUserId,
        CreatorType creatorType, long creatorId, Stream? content, Models.Assets.Type assetType,
        Models.Assets.Genre genre, Models.Assets.ModerationStatus moderationStatus, DateTime? createdAt = null,
        DateTime? updatedAt = null, long? robloxAssetId = 0, bool disableRender = false, long? contentId = null, long? assetIdOverride = null)
    {
        // Validation
        ValidateNameAndDescription(name, description);

        string? contentKey = null;
        if (content != null && contentId == null)
        {
            contentKey = await UploadAssetContent(content, Configuration.AssetDirectory);
        }
        else if (assetType == Type.Package || (content == null && contentId != null))
        {
            // safe
        }
        else
        {
            throw new Exception("Either contentId or stream can be null, but not both");
        }

        long assetId = 0;
        long assetVersionId = 0;
        if (createdAt == null) createdAt = DateTime.UtcNow;
        if (updatedAt == null) updatedAt = createdAt;

        await InTransaction(async (trans) =>
        {
            // check if item was already uploaded before. if true, we can skip moderation check
            if (moderationStatus == ModerationStatus.AwaitingApproval)
            {
                AssetModerationEntry? previouslyUploaded = null;
                if (contentKey != null)
                {
                    previouslyUploaded = await db.QuerySingleOrDefaultAsync<AssetModerationEntry>(
                        "SELECT asset_id as assetId, a.moderation_status as moderationStatus, a.asset_type as assetType FROM asset_version INNER JOIN asset a ON a.id = asset_id WHERE content_url = :url AND a.moderation_status != :status AND NOT a.is_18_plus LIMIT 1", new
                        {
                            status = ModerationStatus.AwaitingApproval,
                            url = contentKey,
                        });
                }
                else if (contentId != null && contentId != 0)
                {
                    previouslyUploaded = await db.QuerySingleOrDefaultAsync<AssetModerationEntry>(
                        "SELECT asset_id as assetId, a.moderation_status as moderationStatus, a.asset_type as assetType FROM asset_version INNER JOIN asset a ON a.id = asset_id WHERE content_id = :id AND a.moderation_status != :status AND NOT a.is_18_plus LIMIT 1", new
                        {
                            status = ModerationStatus.AwaitingApproval,
                            id = contentId.Value,
                        });
                }
                
                if (previouslyUploaded != null)
                {
                    moderationStatus = previouslyUploaded.moderationStatus;
                }
            }

            var request = new Dictionary<string, dynamic?>
            {
                {"roblox_asset_id", robloxAssetId == 0 ? null : robloxAssetId},
                {"name", name},
                {"description", description},
                {"creator_id", creatorId},
                {"creator_type", (int)creatorType},
                {"created_at", createdAt},
                {"updated_at", updatedAt},
                {"moderation_status", (int)moderationStatus},
                {"asset_genre", (int)genre},
                {"asset_type", (int)assetType},
            };
            if (assetIdOverride != null)
                request.Add("id", assetIdOverride);
            
            assetId = await InsertAsync("asset", request);
            if (TypesToGrantOnCreation.Contains(assetType))
            {
                await InsertAsync("user_asset", new
                {
                    asset_id = assetId,
                    user_id = creatorUserId,
                    serial = (int?) null,
                });
            }
            // contentKey = asset url
            // contentId = one asset (e.g. tee shirt image)
            // none = package
            assetVersionId = await InsertAsync("asset_version", new
            {
                asset_id = assetId,
                version_number = 1,
                creator_id = creatorUserId,
                created_at = createdAt,
                updated_at = DateTime.UtcNow,
                content_id = contentId,
                content_url = contentKey,
            });
            if (assetType == Models.Assets.Type.Place)
            {
                // Insert place
                await InsertAsync("asset_place", new
                {
                    asset_id = assetId,
                    max_player_count = 10,
                    server_fill_mode = 1,
                    access = 1,
                    is_vip_enabled = false,
                    is_public_domain = false,
                });
            }


            return 0;
        });

        if (!disableRender)
        {
            RenderAsset(assetId, assetType);
        }

        return new CreateResponse()
        {
            assetId = assetId,
            assetVersionId = assetVersionId,
            moderationStatus = moderationStatus,
        };
    }

    public async Task UpdateAsset(long assetId, string name, string description,
        IEnumerable<Models.Assets.Genre> genres,
        bool enableComments, bool isCopyingAllowed)
    {
        ValidateNameAndDescription(name, description);

        await UpdateAsync("asset", assetId, new
        {
            name,
            description,
            asset_genre = (int) genres.ToArray()[0], // todo: multi genre support
            comments_enabled = enableComments,
        });
    }

    public async Task<CreatePlaceResponse> CreatePlace(long creatorId, CreatorType creatorType, long creatorUserId)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.UploadContentEnabled);
        var basePlateLocation = Configuration.PublicDirectory + "/Baseplate.rbxl";
        await using var basePlateFile = new FileStream(basePlateLocation, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite,
            default, FileOptions.Asynchronous);
        var place = await CreateAsset("Place", null, creatorUserId, creatorType, creatorId, basePlateFile,
            Type.Place, Genre.All, ModerationStatus.ReviewApproved, DateTime.UtcNow, DateTime.UtcNow);
        return new()
        {
            placeId = place.assetId,
        };
    }

    public async Task<ProductEntry> GetProductForAsset(long assetId)
    {
        var result = await db.QuerySingleOrDefaultAsync<ProductEntry>(
            "SELECT name, description,  is_for_sale as isForSale, is_limited as isLimited, is_limited_unique as isLimitedUnique, price_robux as priceRobux, price_tix as priceTickets, serial_count as serialCount, offsale_at as offsaleAt FROM asset WHERE id = :id",
            new
            {
                id = assetId,
            });
        if (result == null) throw new RecordNotFoundException();
        return result;
    }

    public async Task SetItemPrice(long assetId, int? priceRobux, int? priceTickets)
    {
        if (priceRobux is < 0)
            throw new ArgumentException(nameof(priceRobux) + " cannot be less than 0");
        if (priceTickets is < 0)
            throw new ArgumentException(nameof(priceTickets) + " cannot be less than 0");
        
        if (priceTickets == 0)
            priceTickets = null;
        
        
        await db.ExecuteAsync("UPDATE asset SET price_robux = :r, price_tix = :t WHERE id = :id", new
        {
            id = assetId,
            t = priceTickets,
            r = priceRobux,
        });
    }
    
    public async Task UpdateAssetMarketInfo(long assetId, bool isForSale, bool isLimited, bool isLimitedUnique, int? maxCopies, DateTime? offsaleDeadline)
    {
        if (isLimitedUnique && !isLimited)
        {
            isLimited = true;
        }

        await UpdateAsync("asset", assetId, new
        {
            is_for_sale = isForSale,
            is_limited = isLimited,
            is_limited_unique = isLimitedUnique,
            serial_count = maxCopies,
            offsale_at = offsaleDeadline,
            updated_at = DateTime.UtcNow,
        });
    }

    [Obsolete("Use UpdateAssetMarketInfo() without the price to update product data, or SetItemPrice to set the price")]
    public async Task UpdateAssetMarketInfo(long assetId, bool isForSale, bool isLimited, bool isLimitedUnique,
        int? price, int? maxCopies, DateTime? offsaleDeadline)
    {
        if (isLimitedUnique && !isLimited)
        {
            isLimited = true;
        }

        if (price < 0)
            throw new ArgumentException("Price cannot be below zero");

        await UpdateAsync("asset", assetId, new
        {
            is_for_sale = isForSale,
            is_limited = isLimited,
            is_limited_unique = isLimitedUnique,
            price_robux = price,
            serial_count = maxCopies,
            offsale_at = offsaleDeadline,
            updated_at = DateTime.UtcNow,
        });
    }

    // TODO: Description Support
    public async Task UpdateAssetNameAndDesc(long assetId, string newName)
    {
        await UpdateAsync("asset", assetId, new
        {
            name = newName
        });
    }


    public async Task<Dto.Assets.MultiGetEntryLowestSeller?> GetLowestPrice(long assetId)
    {
        var result = await db.QuerySingleOrDefaultAsync<MultiGetEntryLowestSeller>(
            "SELECT user_asset.price as price, asset_id as assetId, user_id as userId, user_asset.id as userAssetId, \"user\".username, user_asset.asset_id as assetId FROM user_asset INNER JOIN \"user\" ON \"user\".id = user_asset.user_id WHERE asset_id = :asset_id AND user_asset.price > 0 ORDER BY price ASC LIMIT 1",
            new {asset_id = assetId});
        return result;
    }

    public async Task<MultiGetEntry> GetAssetCatalogInfo(long assetId)
    {
        var entry = (await MultiGetInfoById(new List<long>() {assetId})).ToList();
        if (entry.Count <= 0) throw new RecordNotFoundException("Asset " + assetId + " does not exist");
        return entry[0];
    }

    private static Dictionary<long, Tuple<DateTime, long>> saleCounts { get; } = new();
    private static Object saleCountsLock { get; } = new();

    public async Task IncrementSaleCount(long assetId)
    {
        var addRequired = false;
        lock (saleCountsLock)
        {
            if (!saleCounts.ContainsKey(assetId))
                addRequired = true;
            else
            {
                var existing = saleCounts[assetId];
                // We don't want cache lasting longer than 1 hour
                if (existing.Item1.AddHours(1) < DateTime.UtcNow)
                {
                    saleCounts.Remove(assetId);
                    return;
                }
                saleCounts[assetId] = new(existing.Item1, existing.Item2 + 1);
            }
        }

        if (addRequired)
        {
            await GetSaleCount(assetId);
        }
    }

    public async Task<long> GetSaleCount(long assetId)
    {
        lock (saleCountsLock)
        {
            if (saleCounts.ContainsKey(assetId))
            {
                var data = saleCounts[assetId];
                if (data.Item1.AddHours(1) < DateTime.UtcNow)
                {
                    Writer.Info(LogGroup.PerformanceDebugging, "remove {0} from sale cache", assetId);
                    saleCounts.Remove(assetId);
                }
                else
                    return data.Item2;
            }
        }

        var result = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM user_transaction ut WHERE ut.asset_id = :asset_id AND ut.type = :sale_type AND ut.sub_type = :sub_sale_type",
            new
            {
                asset_id = assetId,
                sale_type = (int) PurchaseType.Purchase,
                sub_sale_type = (int) TransactionSubType.ItemPurchase,
            });
        lock (saleCountsLock)
        {
            saleCounts[assetId] = new(DateTime.UtcNow, result.total);
        }
        return result.total;
    }

    public async Task<IEnumerable<Dto.Assets.MultiGetEntry>> MultiGetInfoById(IEnumerable<long> assetIds)
    {
        // TODO: If we ever get to a scale where unnecessary joins become an issue, then replace left join code with a switch/case depending on creatorType

        var idsEnumerable = assetIds.ToList();

        if (idsEnumerable.Count <= 0) return Array.Empty<MultiGetEntry>();
        var watch = new Stopwatch();
        watch.Start();
        var query = new SqlBuilder();
        var t = query.AddTemplate(
            "SELECT asset.id as id, asset_type as assetType, asset.name, asset.description, asset_genre as genre, creator_type as creatorType, creator_id as creatorTargetId, offsale_at as offsaleDeadline, is_for_sale as isForSale, price_robux as priceRobux, price_tix as priceTickets, is_limited as isLimited, is_limited_unique as isLimitedUnique, serial_count as serialCount, \"group\".name as groupName, \"user\".username as username, asset.created_at as createdAt, asset.updated_at as updatedAt, asset.is_18_plus, asset.moderation_status FROM asset LEFT JOIN \"user\" ON \"user\".id = asset.creator_id LEFT JOIN \"group\" ON \"group\".id = asset.creator_id /**where**/ LIMIT 200", new
            {
                sale_type = PurchaseType.Purchase,
                sub_sale_type = TransactionSubType.ItemPurchase,
            });
        query.OrWhereMulti("asset.id = $1", idsEnumerable);

        var result = (await db.QueryAsync<Dto.Assets.MultiGetEntryInternal>(t.RawSql, t.Parameters)).ToList();
        watch.Stop();
        Writer.Info(LogGroup.PerformanceDebugging, "it took {0}ms to run MultiGetCatalog query", watch.ElapsedMilliseconds);
        // Get sale counts
        var assetSaleCounts = await Task.WhenAll(result.Select(c => GetSaleCount(c.id)));
        var favoriteCounts = await Task.WhenAll(result.Select(c => CountFavorites(c.id)));
        for (var i = 0; i < result.Count; i++)
        {
            result[i].saleCount = (int)assetSaleCounts[i];
            result[i].favoriteCount = favoriteCounts[i];
        }

        // Get lowest sellers, if available
        var limitedItems = result.Where(c => c.isLimited || c.isLimitedUnique)
            .Where(c => !c.isForSale || c.serialCount != 0 && c.serialCount == c.saleCount).ToList();
        if (limitedItems.Count > 0)
        {
            watch.Restart();
            var multiGetResult = await Task.WhenAll(limitedItems.Select(c => GetLowestPrice(c.id)));
            watch.Stop();
            Writer.Info(LogGroup.PerformanceDebugging , "it took {0}ms to run MultiGetCatalog get lowest price query", watch.ElapsedMilliseconds);
            foreach (var item in multiGetResult)
            {
                if (item == null) continue;
                var exists = result.Find(v => v.id == item.assetId);
                if (exists != null)
                {
                    exists.lowestSellerData = item;
                }
            }
        }

        return result.Select(c => new MultiGetEntry(c));
    }

    public async Task<IEnumerable<RecommendedItemEntry>> GetRecommendedItems(Models.Assets.Type assetType,
        long contextAssetId,
        int limit)
    {
        if (limit is >= 100 or <= 0) limit = 10;


        return await db.QueryAsync<RecommendedItemEntry>(
            @"SELECT asset.id as assetId, asset.name, asset.price_robux as price, asset.creator_id as creatorId, asset.creator_type as creatorType, asset.is_for_sale as isForSale, asset.is_limited as isLimited, asset.is_limited_unique as isLimitedUnique, asset.offsale_at as offsaleDeadline,

(case when asset.creator_type = 1 then ""user"".username else ""group"".name end) as creatorName

FROM asset 

LEFT JOIN ""user"" ON asset.creator_id = ""user"".id AND asset.creator_type = 1 
LEFT JOIN ""group"" ON asset.creator_id = ""group"".id AND asset.creator_type = 2 
                
WHERE asset_type = :asset_type AND asset.id < :id AND NOT asset.is_18_plus ORDER BY asset.id DESC LIMIT :limit",
            new
            {
                asset_type = (int) assetType,
                id = contextAssetId,
                limit,
            });
    }

    private Models.Assets.Type? GetTypeFromPluralString(string pluralString)
    {
        switch (pluralString)
        {
            case "HairAccessories":
                return Type.HairAccessory;
            case "Hat":
            case "Hats":
            case "HatAccessories":
                return Type.Hat;
            case "Faces":
                return Type.Face;
            case "FaceAccessories":
                return Type.FaceAccessory;
            case "NeckAccessories":
                return Type.NeckAccessory;
            case "ShoulderAccessories":
                return Type.ShoulderAccessory;
            case "FrontAccessories":
                return Type.FrontAccessory;
            case "BackAccessories":
                return Type.BackAccessory;
            case "WaistAccessories":
                return Type.WaistAccessory;
            case "Shirts":
                return Type.Shirt;
            case "Pants":
                return Type.Pants;
            case "Tshirts":
                return Type.TeeShirt;
            case "Heads":
                return Type.Head;
        }

        return null;
    }

    public async Task<SearchResponse> SearchCatalog(CatalogSearchRequest request)
    {
        var resp = new SearchResponse();
        resp.keyword = request.keyword;

        // Offset
        var offset = 0;
        if (!string.IsNullOrEmpty(request.cursor))
        {
            offset = int.Parse(request.cursor);
        }

        var builder = new SqlBuilder();
        var selectTemplate = builder.AddTemplate(
            "SELECT id FROM asset /**where**/ /**orderby**/ LIMIT :limit OFFSET :offset", new
            {
                request.limit,
                offset,
            });
        var countTemplate = builder.AddTemplate("SELECT count(*) AS total FROM asset /**where**/");

        // Keyword/Text Search
        if (!string.IsNullOrEmpty(request.keyword))
        {
            builder.Where("asset.name ILIKE :name", new
            {
                name = "%" + request.keyword + "%",
            });
        }

        if (!request.include18Plus)
        {
            builder.Where("NOT asset.is_18_plus");
        }

        if (request.creatorType != null && request.creatorTargetId != null && request.creatorTargetId != 0)
        {
            builder.Where("asset.creator_id = :creator_id AND asset.creator_type = :creator_type", new
            {
                creator_id = request.creatorTargetId.Value,
                creator_type = request.creatorType.Value,
            });
        }

        // Sort
        if (!string.IsNullOrEmpty(request.sortType))
        {
            var column = "updated_at";
            var mode = "desc";
            if (request.sortType == "0")
            {
                // same as above
            }
            else if (request.sortType == "3")
            {
                // updated
                column = "updated_at";
            }
            else if (request.sortType == "4")
            {
                // price: low to high
                column = "price_robux";
                mode = "asc";
            }
            else if (request.sortType == "5")
            {
                // price: high to low
                column = "price_robux";
            }
            else if (request.sortType == "100")
            {
                // favorite count: high to low
            }

            builder.OrderBy(column + " " + mode);
        }

        if (!request.includeNotForSale)
        {
            builder.Where("(asset.is_for_sale = true OR asset.is_limited = true)");
        }

        // If community creations, exclude system account
        if (request.subcategory == "CommunityCreations")
        {
            builder.Where("creator_id != 1");
        }

        var cat = request.category?.ToLower();
        var sub = request.subcategory?.ToLower();
        
        if (cat is "bodyparts" or "bodypart")
        {
            if (sub is "all" or null)
            {
                builder.Where(
                    $"(asset.asset_type = {(int) Models.Assets.Type.Face} OR asset.asset_type = {(int) Models.Assets.Type.LeftArm} OR asset.asset_type = {(int) Models.Assets.Type.RightArm} OR asset.asset_type = {(int) Models.Assets.Type.LeftLeg} OR asset.asset_type = {(int) Models.Assets.Type.RightLeg} OR asset.asset_type = {(int) Models.Assets.Type.Head} OR asset.asset_type = {(int) Models.Assets.Type.Torso})");
            }
        }else if (cat is "gear" or "gears")
        {
            // we ignore subcategory for now. in the future, that will be the gear type (e.g. "ranged" or "explosive")
            builder.Where(
                $"(asset.asset_type = {(int) Models.Assets.Type.Gear})");
        }
        
        if (sub is "accessories" or "communitycreations")
        {
            builder.Where(
                $"(asset.asset_type = {(int) Models.Assets.Type.Hat} OR asset.asset_type = {(int) Models.Assets.Type.HairAccessory} OR asset.asset_type = {(int) Models.Assets.Type.FaceAccessory} OR asset.asset_type = {(int) Models.Assets.Type.FrontAccessory} OR asset.asset_type = {(int) Models.Assets.Type.BackAccessory} OR asset.asset_type = {(int) Models.Assets.Type.WaistAccessory} OR asset.asset_type = {(int) Models.Assets.Type.ShoulderAccessory} OR asset.asset_type = {(int) Models.Assets.Type.NeckAccessory})");
        }
        else if (sub is "faces")
        {
            builder.Where($"asset.asset_type = {(int) Models.Assets.Type.Face}");
        }
        else if (sub is "clothing")
        {
            builder.Where(
                $"(asset.asset_type = {(int) Models.Assets.Type.Shirt} OR asset.asset_type = {(int) Models.Assets.Type.Pants} OR asset.asset_type = {(int) Models.Assets.Type.TeeShirt})");
        }
        else if (sub is "bodyparts")
        {
            builder.Where(
                $"(asset.asset_type = {(int) Models.Assets.Type.Face} OR asset.asset_type = {(int) Models.Assets.Type.LeftArm} OR asset.asset_type = {(int) Models.Assets.Type.RightArm} OR asset.asset_type = {(int) Models.Assets.Type.LeftLeg} OR asset.asset_type = {(int) Models.Assets.Type.RightLeg} OR asset.asset_type = {(int) Models.Assets.Type.Head} OR asset.asset_type = {(int) Models.Assets.Type.Torso})");
        }
        else if (sub is "packages" or "package")
        {
            builder.Where(
                $"(asset.asset_type = {(int) Models.Assets.Type.Package})");
        }
        else if (sub != "collectibles")
        {
            Models.Assets.Type type;
            if (Enum.TryParse<Models.Assets.Type>(request.subcategory, out type))
            {
                builder.Where($"asset.asset_type = {(int) type}");
            }
            else
            {
                var otherType = GetTypeFromPluralString(request.subcategory);
                if (otherType != null)
                {
                    builder.Where($"asset.asset_type = {(int) otherType}");
                }
            }
        }

        // Whether to sort the final results by ID in DESC order, after the function is over
        var doIdSort = false;

        if (!string.IsNullOrEmpty(request.category))
        {
            if (cat is "communitycreations")
            {
                // TODO: This blocks groupId 1. Is that an issue?
                builder.Where("(asset.creator_id != 1)");
            }
            else if (cat is "collectibles")
            {
                builder.Where("asset.is_limited = true");
            }
            else if (cat is "featured")
            {
                doIdSort = true;
                builder.Where("asset.creator_id = 1").Where("asset.creator_type = 1");
                // TODO: this used to have clothing filters but I got rid of them in the name of performance
                // Exact filters are at /services/api/src/controllers/proxy/v1/Catalog.ts:862
            }
        }

        if (request.genres != null)
        {
            foreach (var item in request.genres)
            {
                builder.Where($"asset.asset_genre = {(int) item}");
            }
        }
        
        var totalResults =
            await db.QuerySingleOrDefaultAsync<Total>(countTemplate.RawSql, countTemplate.Parameters);
        if (totalResults.total != 0)
        {
            resp.data =
                (await db.QueryAsync<CatalogMultiGetEntry>(selectTemplate.RawSql, selectTemplate.Parameters))
                .Select(
                    c => new CatalogMultiGetEntry()
                    {
                        id = c.id,
                        itemType = "Asset",
                    });
        }

        if (resp.data == null)
            return new SearchResponse() {keyword = request.keyword};

        var sortedList = resp.data.ToList();
        if (doIdSort)
        {
            sortedList.Sort((a, b) => a.id > b.id ? -1 : 1);
        }

        if (sortedList.Count >= request.limit)
        {
            resp.nextPageCursor = (sortedList.Count + offset).ToString();
        }

        if (offset != 0)
        {
            resp.previousPageCursor = (offset - request.limit).ToString();
        }

        resp._total = totalResults.total;
        resp.data = sortedList;
        return resp;
    }

    public async Task<IEnumerable<MultiGetAssetDeveloperDetails>> MultiGetAssetDeveloperDetails(
        IEnumerable<long> ids)
    {
        var assets = ids.ToList();
        if (assets.Count == 0) return new List<MultiGetAssetDeveloperDetails>();

        var builder = new SqlBuilder();
        var selectTemplate = builder.AddTemplate(
            "SELECT id as assetId, asset_type as typeId, asset_genre as genre, creator_type as creatorType, creator_id as creatorId, name, description, created_at as created, updated_at as updated, comments_enabled as enableComments, asset.moderation_status as moderationStatus, asset.is_18_plus as is18Plus FROM asset /**where**/");
        for (var i = 0; i < assets.Count; i++)
        {
            var sqlParams = new DynamicParameters();
            sqlParams.Add("param" + i, assets[i]);
            builder.OrWhere("id = @param" + i, sqlParams);
        }

        var result =
            await db.QueryAsync<MultiGetAssetDeveloperDetailsDb>(selectTemplate.RawSql, selectTemplate.Parameters);
        return result.Select(c => new MultiGetAssetDeveloperDetails(c));
    }

    public async Task UpdateAsset(long assetId, string? description, string name, Genre genre,
        bool isCopyingAllowed, bool areCommentsAllowed)
    {
        ValidateNameAndDescription(name, description);

        await UpdateAsync("asset", assetId, new
        {
            name,
            description,
            asset_genre = (int) genre,
            comments_enabled = areCommentsAllowed,
            // is_copying_allowed = isCopyingAllowed,
        });
    }

    public async Task UpdateItemIsForSale(long assetId, bool isForSale)
    {
        await UpdateAsync("asset", assetId, new
        {
            is_for_sale = isForSale,
        });
    }

    private async Task<AssetResaleCharts> GetAssetResaleCharts(long assetId)
    {
        var pricePoints = new List<AssetResaleChartEntry>();
        var volumePoints = new List<AssetResaleChartEntry>();

        var saleHistory = await db.QueryAsync<AssetResaleChartEntry>(
            "SELECT amount as value, created_at as date FROM collectible_sale_logs WHERE asset_id = :id", new
            {
                id = assetId,
            });
        // Round to nearest day
        var salesDict = new Dictionary<DateTime, AssetResaleChartEntry>();
        var volumeDict = new Dictionary<DateTime, AssetResaleChartEntry>();
        foreach (var item in saleHistory)
        {
            var nearestDay = item.date.Date;
            nearestDay = nearestDay.Add(TimeSpan.FromHours(5));
            if (nearestDay >= DateTime.UtcNow)
                continue;
            
            if (!salesDict.ContainsKey(nearestDay))
            {
                salesDict[nearestDay] = new AssetResaleChartEntry();
                volumeDict[nearestDay] = new AssetResaleChartEntry();
            }

            salesDict[nearestDay].value += item.value;
            volumeDict[nearestDay].value++;
        }

        foreach (var item in salesDict)
        {
            var volume = volumeDict[item.Key];
            var averagePrice = item.Value.value / volume.value;
            if (averagePrice <= 0)
                averagePrice = 1;

            pricePoints.Add(new AssetResaleChartEntry()
            {
                date = item.Key,
                value = averagePrice,
            });
            volumePoints.Add(new AssetResaleChartEntry()
            {
                date = item.Key,
                value = volume.value,
            });
        }

        return new()
        {
            priceDataPoints = pricePoints,
            volumeDataPoints = volumePoints,
        };
    }

    public async Task<AssetResaleData> GetResaleData(long assetId)
    {
        var info = await db.QuerySingleOrDefaultAsync<AssetResaleData>(
            "SELECT sale_count as sales, serial_count as assetStock, recent_average_price as recentAveragePrice, price_robux as originalPrice FROM asset WHERE id = :id",
            new
            {
                id = assetId,
            });
        using (var us = ServiceProvider.GetOrCreate<UsersService>())
        {
            info.sales = await us.CountSoldCopiesForAsset(assetId);
        }
        if (info.assetStock != 0)
        {
            info.numberRemaining = info.assetStock - info.sales;
        }

        var charts = await GetAssetResaleCharts(assetId);
        info.priceDataPoints = charts.priceDataPoints;
        info.volumeDataPoints = charts.volumeDataPoints;

        return info;
    }

    /// <summary>
    /// Validate write (and read) permissions for the assetId.
    /// </summary>
    /// <param name="assetId"></param>
    /// <param name="userId"></param>
    public async Task ValidatePermissions(long assetId, long userId)
    {
        if (await CanUserModifyItem(assetId, userId)) return;

        throw new PermissionException(assetId, userId);
    }

    public async Task<bool> CanUserModifyItem(long assetId, long userId)
    {
        // todo: move IsOwner() to service
        if (userId == 3) return true;
        
        var details = await GetAssetCatalogInfo(assetId);
        switch (details.creatorType)
        {
            case CreatorType.User:
                return details.creatorTargetId == userId;
            case CreatorType.Group:
            {
                using var gs = ServiceProvider.GetOrCreate<GroupsService>(this);
                var role = await gs.GetUserRoleInGroup(details.creatorTargetId, userId);
                return role.HasPermission(GroupPermission.ManageItems);
            }
            default:
                throw new Exception("Unsupported creatorType: " + details.creatorType);
        }

        return false;
    }

    public async Task<IEnumerable<CreationEntry>> GetCreations(CreatorType creatorType, long creatorId,
        Type assetType,
        int offset, int limit)
    {
        return await db.QueryAsync<CreationEntry>(
            "SELECT asset.id as assetId, asset.name as name FROM asset WHERE creator_type = :creator_type AND creator_id = :creator_id AND asset_type = :asset_type ORDER BY id DESC LIMIT :limit OFFSET :offset",
            new
            {
                limit = limit,
                offset = offset,
                creator_id = creatorId,
                creator_type = (int) creatorType,
                asset_type = (int) assetType,
            });
    }

    public async Task<bool> AreCommentsEnabled(long assetId)
    {
        var result = await db.QuerySingleOrDefaultAsync("SELECT comments_enabled FROM asset WHERE id = :id", new
        {
            id = assetId,
        });
        return result.comments_enabled;
    }

    public async Task<IEnumerable<CommentEntry>> GetComments(long assetId, int offset, int limit)
    {
        return await db.QueryAsync<CommentEntry>(
            "SELECT asset_comment.id, asset_comment.created_at as createdAt, u.username, asset_comment.user_id as userId, asset_comment.comment as comment FROM asset_comment INNER JOIN \"user\" u ON u.id = asset_comment.user_id WHERE asset_comment.asset_id = :id ORDER BY asset_comment.id desc LIMIT :limit OFFSET :offset",
            new {limit = limit, offset = offset, id = assetId});
    }

    public async Task<bool> IsInCommentCooldown(long userId)
    {
        var totalComments = await db.QuerySingleOrDefaultAsync<Total>(
            "SELECT COUNT(*) AS total FROM asset_comment WHERE user_id = :user_id AND created_at >= :dt", new
            {
                dt = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5)),
                user_id = userId,
            });
        return totalComments.total >= 5;
    }

    public async Task<bool> IsInCommentCooldownGlobal()
    {
        var totalComments = await db.QuerySingleOrDefaultAsync<Total>(
            "SELECT COUNT(*) AS total FROM asset_comment WHERE created_at >= :dt", new
            {
                dt = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5)),
            });
        // TODO: Might wanna play around with this - is it big enough?
        return totalComments.total >= 25;
    }


    private static Regex commentRegex = new("[a-zA-Z0-9]+");

    private bool IsCommentValid(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment)) return false;
        var match = commentRegex.Matches(comment);
        if (match.Count == 0)
            return false;
        
        var m = "";
        for (var i = 0; i < match.Count; i++)
        {
            m += match[i].Value;
        }

        return m.Length >= 3;
    }

    public async Task AddComment(long assetId, long userId, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            throw new ArgumentException("Comment is null or empty");
        if (!IsCommentValid(comment))
            throw new ArgumentException("Comment is too short. It must be at least 3 alpha-numeric characters");
        if (comment.Length > 200) throw new ArgumentException("Comment is too long");

        var details = (await MultiGetAssetDeveloperDetails(new[] {assetId})).First();
        if (!details.enableComments)
            throw new ArgumentException("Asset does not support comments");
        if (await IsInCommentCooldown(userId)) throw new FloodcheckException();
        if (await IsInCommentCooldownGlobal()) throw new FloodcheckException();
        await InsertAsync("asset_comment", new
        {
            comment = comment,
            user_id = userId,
            asset_id = assetId,
        });
    }

    public async Task DeleteAsset(long assetId)
    {
        // if you are editing asset tables or columns, remember to add deletion info to this table
        await InTransaction(async (trx) =>
        {
            var versions = await db.QueryAsync<AssetVersionEntry>(
                "SELECT content_url as contentUrl, id as assetVersionId FROM asset_version WHERE asset_id = :id",
                new {id = assetId});
            // delete asset versions
            foreach (var version in versions)
            {
                if (version.contentUrl == null) continue;
                // make sure no other assets depend on it
                var refs = await db.QuerySingleOrDefaultAsync<Total>(
                    "SELECT COUNT(*) as total FROM asset_version WHERE content_url = :url",
                    new {url = version.contentUrl});
                if (refs.total <= 1)
                {
                    // we can safely delete it - only person depending on it is current asset
                    await DeleteAssetContent(version.contentUrl, Configuration.AssetDirectory);
                }
                else
                {
                    Console.WriteLine(
                        "[info] In deletion of {0}: Not deleting avid {1} as {2} assets reference it", assetId,
                        version.assetVersionId, refs.total - 1);
                }
            }

            // delete thumbnails
            var allThumbnails =
                await db.QueryAsync("SELECT content_url FROM asset_thumbnail WHERE asset_id = :id",
                    new {id = assetId});
            foreach (var thumb in allThumbnails)
            {
                var url = (string?) thumb.content_url;
                if (url != null)
                {
                    await DeleteAssetContent(url + ".png", Configuration.ThumbnailsDirectory);
                }
            }

            // now mostly db ops
            await db.ExecuteAsync("DELETE FROM user_avatar_asset WHERE asset_id = :asset_id", new
            {
                asset_id = assetId
            });
            await db.ExecuteAsync("DELETE FROM user_outfit_asset WHERE asset_id = :asset_id", new
            {
                asset_id = assetId,
            });
            await db.ExecuteAsync("DELETE FROM user_asset WHERE asset_id = :asset_id", new
            {
                asset_id = assetId,
            });
            await db.ExecuteAsync("DELETE FROM asset_comment WHERE asset_id = :asset_id", new
            {
                asset_id = assetId,
            });
            await db.ExecuteAsync("DELETE FROM asset_thumbnail WHERE asset_id = :asset_id", new
            {
                asset_id = assetId,
            });
            await db.ExecuteAsync("DELETE FROM asset_version WHERE asset_id = :asset_id", new
            {
                asset_id = assetId,
            });
            // finally, delete the asset itself
            await db.ExecuteAsync("DELETE FROM asset WHERE id = :asset_id", new
            {
                asset_id = assetId,
            });
            return 0;
        });
    }

    private async Task<UserAdvertisementType> ParseAdvertisementImage(Stream image)
    {
        using var imageData = await SixLabors.ImageSharp.Image.LoadAsync(image);
        if (imageData == null) throw new ArgumentException("Bad image");

        if (imageData.Width == 728 && imageData.Height == 90)
        {
            return UserAdvertisementType.Banner728x90;
        }

        if (imageData.Width == 160 && imageData.Height == 600)
        {
            return UserAdvertisementType.SkyScraper160x600;
        }

        if (imageData.Width == 300 && imageData.Height == 250)
        {
            return UserAdvertisementType.Rectangle300x250;
        }

        // Unknown size
        throw new ArgumentException("Unknown image dimensions");
    }

    private const string UserAdColumns =
        "asset_advertisement.id, asset_advertisement.target_id as targetId, asset_advertisement.target_type as targetType, asset_advertisement.created_at as createdAt, asset_advertisement.updated_at as updatedAt, advertisement_type as advertisementType, advertisement_asset_id as advertisementAssetId, impressions_all as impressionsAll, clicks_all as clicksAll, bid_amount_robux_all as bidAmountRobuxAll, impressions_last_run as impressionsLastRun, clicks_last_run as clicksLastRun, bid_amount_robux_last_run as bidAmountRobuxLastRun, asset_advertisement.name";

    public async Task<IEnumerable<AdvertisementEntry>> GetAdvertisementsForAsset(long assetId)
    {
        return await db.QueryAsync<AdvertisementEntry>(
            "SELECT " + UserAdColumns +
            " FROM asset_advertisement WHERE target_id = :asset_id AND target_type = :target_type",
            new {asset_id = assetId, target_type = UserAdvertisementTargetType.Asset});
    }

    public async Task<IEnumerable<AdvertisementEntry>> GetAdvertisementsByUser(long userId)
    {
        return await db.QueryAsync<AdvertisementEntry>(
            "SELECT " + UserAdColumns +
            " FROM asset_advertisement WHERE target_id IN (SELECT id FROM asset WHERE asset.creator_type = 1 AND asset.creator_id = :user_id) AND target_type = :asset",
            new {user_id = userId, asset = UserAdvertisementTargetType.Asset});
    }

    public async Task<IEnumerable<AdvertisementEntry>> GetAdvertisementsByGroup(long groupId)
    {
        // Get advertisements for assets owned by the group, as well as ads for the group itself
        return await db.QueryAsync<AdvertisementEntry>(
            "SELECT " + UserAdColumns +
            " FROM asset_advertisement WHERE target_id IN (SELECT id FROM asset WHERE asset.creator_type = 2 AND asset.creator_id = :id) AND target_type = :asset_type OR (asset_advertisement.target_id = :id AND asset_advertisement.target_type = :group_type)",
            new
            {
                asset_type = UserAdvertisementTargetType.Asset,
                group_type = UserAdvertisementTargetType.Group,
                id = groupId,
            });
    }

    public async Task<AdvertisementEntry> GetAdvertisementById(long advertisementId)
    {
        var details = await db.QuerySingleOrDefaultAsync<AdvertisementEntry>(
            "SELECT " + UserAdColumns + " FROM asset_advertisement WHERE id = :id", new {id = advertisementId});
        if (details == null) throw new RecordNotFoundException();
        return details;
    }

    public async Task IncrementAdvertisementClick(long advertisementId)
    {
        await db.ExecuteAsync(
            "UPDATE asset_advertisement SET clicks_last_run = clicks_last_run + 1, clicks_all = clicks_all + 1 WHERE id = :id",
            new
            {
                id = advertisementId,
            });
    }


    public async Task IncrementAdvertisementImpressions(long advertisementId)
    {
        await db.ExecuteAsync(
            "UPDATE asset_advertisement SET impressions_last_run = impressions_last_run + 1, impressions_all = impressions_all + 1 WHERE id = :id",
            new
            {
                id = advertisementId,
            });
    }

    public async Task<IEnumerable<AdvertisementEntry>> GetAdPool(UserAdvertisementType type)
    {
        return await db.QueryAsync<AdvertisementEntry>(
            "SELECT " + UserAdColumns +
            " FROM asset_advertisement LEFT JOIN asset a ON a.id = advertisement_asset_id WHERE advertisement_type = :type AND asset_advertisement.updated_at >= :updated_at AND bid_amount_robux_last_run > 0 AND a.moderation_status = :status",
            new
            {
                type = type,
                updated_at = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)),
                status = ModerationStatus.ReviewApproved,
            });
    }

    public async Task<AdvertisementEntry?> GetAdvertisementForIFrame(UserAdvertisementType type, long? userId)
    {
        var details = (await GetAdPool(type)).ToArray();

        if (details.Length == 0) return null;
        if (details.Length == 1) return details[0]; // prevent out of range exception
        var allow18Plus = false;
        if (userId != null)
        {
            allow18Plus = await ServiceProvider.GetOrCreate<UsersService>(this).Is18Plus(userId.Value);
        }
        // create a list. put one entry in list for each robux bid on each ad
        var adIds = new List<long>();
        var idx = 0;
        using var assets = ServiceProvider.GetOrCreate<AssetsService>(this);
        foreach (var item in details)
        {
            var isAd18Plus = await assets.Is18Plus(item.advertisementAssetId);
            if (isAd18Plus && !allow18Plus)
                continue;
            
            for (long i = 0; i < item.bidAmountRobuxLastRun; i++)
            {
                adIds.Add(idx);
            }

            idx++;
        }

        var pickedIdx = new Random().Next(adIds.Count);
        var adId = adIds[pickedIdx];
        return details[adId];
    }

    public async Task RunAdvertisement(long contextUserId, long advertisementId, long bidAmount)
    {
        // TODO: allow funding an ad through group funds. Not possible right now since group funds don't exist.
        if (bidAmount <= 0) throw new RobloxException(RobloxException.BadRequest, 0, "BadRequest");
        await using var redLock = await Cache.redLock.CreateLockAsync("ToggleAdvertisement:V1:" + advertisementId,
            TimeSpan.FromMinutes(1));
        if (!redLock.IsAcquired) throw new LockNotAcquiredException();

        await InTransaction(async (trx) =>
        {
            var details = await GetAdvertisementById(advertisementId);
            if (details.isRunning)
            {
                throw new ArgumentException("Cannot run an ad that is already running");
            }

            var imageDetails = (await MultiGetAssetDeveloperDetails(new[] {details.advertisementAssetId})).First();
            if (imageDetails.moderationStatus != ModerationStatus.ReviewApproved)
                throw new RobloxException(RobloxException.BadRequest, 0,
                    "BadRequest"); // cannot run an ad that hasn't been approved

            if (details.targetType == UserAdvertisementTargetType.Asset)
            {
                // confirm permissions
                await ValidatePermissions(details.targetId, contextUserId);
            }
            else if (details.targetType == UserAdvertisementTargetType.Group)
            {
                var gs = new GroupsService();
                var perms = await gs.GetUserRoleInGroup(details.targetId, contextUserId);
                if (!perms.HasPermission(GroupPermission.AdvertiseGroup))
                {
                    throw new RobloxException(RobloxException.Forbidden, 0, "Forbidden");
                }

                // Confirm not locked
                var groupData = await gs.GetGroupById(details.targetId);
                if (groupData.isLocked)
                {
                    throw new RobloxException(RobloxException.Forbidden, 0, "Forbidden");
                }
            }
            else
            {
                throw new NotImplementedException("targetType not supported: " + details.targetType);
            }

            var balance = await db.QuerySingleOrDefaultAsync<UserEconomy>(
                "SELECT balance_robux as robux FROM user_economy WHERE user_id =:id", new {id = contextUserId});
            if (balance.robux < bidAmount)
                throw new ArgumentException("User does not enough Robux to purchase this ad");
            // Start the ad
            await db.ExecuteAsync(
                "UPDATE asset_advertisement SET updated_at = :updated_at, bid_amount_robux_all = bid_amount_robux_all + :amt, bid_amount_robux_last_run = :amt, impressions_last_run = 0, clicks_last_run = 0, bid_amount_tix_last_run = 0 WHERE id = :id",
                new
                {
                    amt = bidAmount,
                    updated_at = DateTime.UtcNow,
                    id = advertisementId,
                });
            // Charge user
            await db.ExecuteAsync(
                "UPDATE user_economy SET balance_robux = balance_robux - :amt WHERE user_id = :id", new
                {
                    id = contextUserId,
                    amt = bidAmount,
                });
            // Create transaction
            await InsertAsync("user_transaction", new
            {
                user_id_one = contextUserId,
                user_id_two = 1,
                amount = bidAmount,
                type = PurchaseType.AdSpend,
                currency_type = 1,
            });
            return 0;
        });
    }

    private static readonly List<Models.Assets.Type> allowedAssetTypesForAdvertisements = new List<Type>()
    {
        Type.TeeShirt,
        Type.Shirt,
        Type.Pants,
        Type.Place,
    };

    public async Task CreateAdvertisement(long contextUserId, long targetId, UserAdvertisementTargetType targetType,
        string advertisementName, Stream file)
    {
        // name
        if (string.IsNullOrWhiteSpace(advertisementName) || advertisementName.Length is < 3 or > 64)
            throw new RobloxException(RobloxException.BadRequest, 10, "Invalid ad name");
        if (file.Length > 4e+6)
        {
            throw new RobloxException(RobloxException.BadRequest, 0, "Invalid file");
        }

        // perms/validation specific to each targetType
        if (targetType == UserAdvertisementTargetType.Asset)
        {
            await ValidatePermissions(targetId, contextUserId);

            // confirm exists & mod status is ok
            var itemInfo = (await MultiGetAssetDeveloperDetails(new[] {targetId})).First();
            if (!allowedAssetTypesForAdvertisements.Contains((Type) itemInfo.typeId))
            {
                throw new RobloxException(RobloxException.BadRequest, 0, "BadRequest");
            }

            if (itemInfo.moderationStatus != ModerationStatus.ReviewApproved)
                throw new RobloxException(RobloxException.BadRequest, 2, "Ad target is not approved");
        }
        else if (targetType == UserAdvertisementTargetType.Group)
        {
            var gs = new GroupsService();
            var perms = await gs.GetUserRoleInGroup(targetId, contextUserId);
            if (!perms.HasPermission(GroupPermission.AdvertiseGroup))
            {
                throw new RobloxException(RobloxException.Forbidden, 0, "Forbidden");
            }

            // Confirm not locked
            var groupData = await gs.GetGroupById(targetId);
            if (groupData.isLocked)
            {
                throw new RobloxException(RobloxException.Forbidden, 0, "Forbidden");
            }
        }
        else
        {
            throw new NotImplementedException("No supported for targetType: " + targetType);
        }

        var existingAds = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM asset_advertisement WHERE target_id = :id AND target_type = :type", new
            {
                id = targetId,
                type = targetType,
            });
        if (existingAds.total >= 100)
        {
            throw new RobloxException(RobloxException.BadRequest, 0,
                targetType + " has reached maximum advertisement count");
        }

        UserAdvertisementType? type = null;
        try
        {
            type = await ParseAdvertisementImage(file);
        }
        catch (Exception)
        {
            // todo: log?
            throw new RobloxException(RobloxException.BadRequest, 6, "Invalid image size");
        }

        file.Position = 0;
        // upload image
        var image = await CreateAsset(advertisementName, null, contextUserId, CreatorType.User, contextUserId, file,
            Type.Image,
            Genre.All, ModerationStatus.AwaitingApproval, default, default, default, true);
        // disable render above, then manually render it.
        // awaited since it's unlikely this would take more than a second
        await CreateRawImageThumbnail(image.assetId);
        // insert ad
        await InsertAsync("asset_advertisement", new
        {
            name = advertisementName,
            target_type = targetType,
            target_id = targetId,
            advertisement_type = type,
            advertisement_asset_id = image.assetId,
        });
    }

    public async Task InsertOrUpdateAssetVersionMetadataImage(long assetVersionId, int sizeBytes, int resolutionX, int resolutionY,
        ImagerFormat format, string hash)
    {
        await db.ExecuteAsync("INSERT INTO asset_version_metadata_image (asset_version_id, resolution_x, resolution_y, image_format, hash, size_bytes) VALUES (:id, :x, :y, :format, :hash, :size_bytes) ON CONFLICT (asset_version_id) DO UPDATE SET resolution_x = :x, resolution_y = :y, image_format = :format, hash = :hash, size_bytes = :size_bytes", new
        {
            id = assetVersionId,
            x = resolutionX,
            y = resolutionY,
            format = format,
            hash = hash,
            size_bytes = sizeBytes,
        });
    }

    public async Task<IEnumerable<AssetVersionEntry>> GetAssetImagesWithoutMetadata()
    {
        var all = await db.QueryAsync<AssetVersionEntry>(
            "SELECT asset_version.id as assetVersionId, content_id as contentId, content_url as contentUrl, asset_id as assetId FROM asset_version INNER JOIN asset ON asset.id = asset_version.asset_id LEFT JOIN asset_version_metadata_image avmi on asset_version.id = avmi.asset_version_id WHERE avmi.created_at IS NULL AND (asset.asset_type = 1) AND asset_version.content_url IS NOT NULL");
        return all;
    }

    public async Task<String> GenerateImageHash(Stream content)
    {
        if (content.Position != 0)
            content.Position = 0;
        
        var sha256 = SHA256.Create();
        var bin = await sha256.ComputeHashAsync(content);
        content.Position = 0;
        return Convert.ToHexString(bin).ToLowerInvariant();
    }

    public async Task FixAssetImagesWithoutMetadata()
    {
        try
        {
            Writer.Info(LogGroup.FixAssetImageMetadata, "fixing thumbnails");
            var w = new Stopwatch();
            w.Start();
            var toFix = await GetAssetImagesWithoutMetadata();
            w.Stop();
            var list = toFix.ToArray();
            Writer.Info(LogGroup.FixAssetImageMetadata, "took {0}ms to get all asset versions to fix. length is {1}", w.ElapsedMilliseconds, list.Length);
            foreach (var version in list)
            {
                // Writer.Info(LogGroup.FixAssetImageMetadata, "fixing {0}", version.assetVersionId);
                if (version.contentUrl is null) continue;
                Imager info;
                Stream data;
                try
                {
                    data = await GetAssetContent(version.contentUrl);
                    info = await Imager.ReadAsync(data);
                }
                catch (Exception e)
                {
                    Writer.Info(LogGroup.FixAssetImageMetadata, "error reading image for avid={0}: {1}\n{2}", version.assetVersionId, e.Message, e.StackTrace);
                    await DeleteAsset(version.assetId);
                    continue;
                }
                await InsertOrUpdateAssetVersionMetadataImage(version.assetVersionId, (int)data.Length, info.width, info.height, info.imageFormat, await GenerateImageHash(data));
            }
            Writer.Info(LogGroup.FixAssetImageMetadata, "done fixing images");
        }
        catch (Exception e)
        {
            Writer.Info(LogGroup.FixAssetImageMetadata, "fatal error fixing images: {0}\n{1}", e.Message, e.StackTrace);
        }
    }

    public async Task<long> CountAssetsPendingApproval()
    {
        // SELECT COUNT(*) AS total FROM asset WHERE moderation_status = 2 AND asset_type != 11 AND asset_type != 12;
        var result = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM asset WHERE moderation_status = :mod_status AND asset_type != :shirt AND asset_type != :pants AND asset_type != :special", new
            {
                // special, so dont count them
                shirt = Models.Assets.Type.Shirt,
                pants = Models.Assets.Type.Pants,
                special = Models.Assets.Type.Special,
                
                mod_status = ModerationStatus.AwaitingApproval,
            });
        return result.total;
    }

    public async Task<long> CountAssetsByCreatorPendingApproval(long creatorId, CreatorType creatorType)
    {
        var result = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM asset WHERE moderation_status = :mod_status AND creator_id = :id AND creator_type = :type",
            new
            {
                type = creatorType,
                id = creatorId,
                mod_status = ModerationStatus.AwaitingApproval,
            });
        return result.total;
    }

    private async Task<long> GetVoteCount(long assetId, AssetVoteType type)
    {
        var votes = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM asset_vote WHERE asset_id = :id AND type = :type", new
            {
                id = assetId,
                type = type,
            });
        return votes.total;
    }

    public async Task<AssetVotesResponse> GetVoteForAsset(long assetId)
    {
        var upVotes = await GetVoteCount(assetId, AssetVoteType.Upvote);
        var downVotes = await GetVoteCount(assetId, AssetVoteType.Downvote);
        return new AssetVotesResponse()
        {
            upVotes = upVotes,
            downVotes = downVotes,
        };
    }

    private async Task<bool> HasUserVisitedPlace(long userId, long placeId)
    {
        var result = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) as total FROM asset_play_history WHERE user_id = :user_id AND asset_id = :asset_id",
            new
            {
                asset_id = placeId,
                user_id = userId,
            });
        return result.total != 0;
    }

    public async Task<long> GetVotesForUser(long userId, TimeSpan since)
    {
        var t = DateTime.UtcNow.Subtract(since);
        var total = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM asset_vote WHERE user_id = :user_id AND created_at >= :created_at", new
            {
                user_id = userId,
                created_at = t,
            });
        return total.total;
    }

    public async Task<long> GetVotesForPlace(long assetId, TimeSpan since)
    {
        var t = DateTime.UtcNow.Subtract(since);
        var total = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM asset_vote WHERE asset_id = :asset_id AND created_at >= :created_at", new
            {
                asset_id = assetId,
                created_at = t,
            });
        return total.total;
    }

    public async Task VoteOnAsset(long assetId, long userId, bool isUpvote)
    {
        var details = await GetAssetCatalogInfo(assetId);
        if (details.assetType != Type.Place)
            throw new RobloxException(400, 3, "Invalid asset id");
        // Confirm user has been to this place before
        if (!await HasUserVisitedPlace(userId, assetId))
            throw new RobloxException(403, 6, "Requester must play this game before they can vote");
        // Acquire lock to prevent duplicate votes
        await using var redlock =
            await Cache.redLock.CreateLockAsync("VoteOnAssetLockV1:" + userId, TimeSpan.FromMinutes(1));

        if (!redlock.IsAcquired)
            throw new LockNotAcquiredException();

        // Confirm user isn't spamming votes.
        // 10 in a 5 minute period
        // 100 in a day
        var c = await GetVotesForUser(userId, TimeSpan.FromMinutes(5));
        if (c >= 10)
        {
            Metrics.GameMetrics.ReportFloodCheckForVoteShort(userId, assetId);
            throw new RobloxException(429, 0, "TooManyRequests");
        }

        if (await GetVotesForUser(userId, TimeSpan.FromDays(1)) >= 100)
        {
            Metrics.GameMetrics.ReportFloodCheckForVoteLong(userId, assetId);
            throw new RobloxException(429, 0, "TooManyRequests");
        }

        // 100 in a day. This is probably too low but will have to work for now.
        if (await GetVotesForPlace(assetId, TimeSpan.FromDays(1)) >= 100)
        {
            Metrics.GameMetrics.ReportFloodCheckForAsset(assetId);
            throw new RobloxException(429, 0, "TooManyRequests");
        }

        var t = isUpvote ? AssetVoteType.Upvote : AssetVoteType.Downvote;

        // If the vote already exists, don't do anything.
        var voteAlreadyExists = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM asset_vote WHERE user_id = :user_id AND asset_id = :asset_id AND type = :type",
            new
            {
                type = t,
                user_id = userId,
                asset_id = assetId,
            });
        if (voteAlreadyExists.total != 0)
            return;

        // Delete any existing
        await db.ExecuteAsync("DELETE FROM asset_vote WHERE user_id = :user_id AND asset_id = :asset_id", new
        {
            asset_id = assetId,
            user_id = userId,
        });
        // Insert
        await db.ExecuteAsync(
            "INSERT INTO asset_vote (user_id, asset_id, type, created_at, updated_at) VALUES (:user_id, :asset_id, :type, :created_at, :created_at)",
            new
            {
                user_id = userId,
                asset_id = assetId,
                type = t,
                created_at = DateTime.UtcNow,
            });
    }

    public async Task InsertPackageAsset(long packageAssetId, long assetId)
    {
        await db.ExecuteAsync("INSERT INTO asset_package (package_asset_id, asset_id) VALUES (:package, :asset)", new
        {
            asset = assetId,
            package = packageAssetId,
        });
    }

    public async Task<IEnumerable<long>> GetPackageAssets(long assetId)
    {
        var result = await db.QueryAsync("SELECT asset_id FROM asset_package WHERE package_asset_id = :id", new
        {
            id = assetId
        });
        return result.Select(c => (long) c.asset_id);
    }

    public async Task<long> CountFavorites(long assetId)
    {
        var result = await db.QuerySingleOrDefaultAsync<Dto.Total>("SELECT COUNT(*) AS total FROM asset_favorite WHERE asset_id = :id", new
        {
            id = assetId
        });
        return result.total;
    }

    public async Task<FavoriteEntry?> GetFavoriteStatus(long userId, long assetId)
    {
        var result = await db.QuerySingleOrDefaultAsync<FavoriteEntry>(
            "SELECT user_id as userId, asset_id as assetId, created_at as createdAt FROM asset_favorite WHERE user_id = :user_id AND asset_id = :asset_id",
            new
            {
                user_id = userId,
                asset_id = assetId,
            });
        return result;
    }

    public async Task<IEnumerable<FavoriteEntry>> GetFavoritesOfType(long userId, Models.Assets.Type assetType,
        int limit, int offset)
    {
        var result = await db.QueryAsync<FavoriteEntry>("SELECT user_id as userId, asset_id as assetId, asset_favorite.created_at as created FROM asset_favorite INNER JOIN asset ON asset.id = asset_favorite.asset_id WHERE asset_favorite.user_id = :user_id AND asset.asset_type = :asset_type ORDER BY created DESC LIMIT :limit OFFSET :offset", new
        {
            user_id = userId,
            asset_type = assetType,
            limit = limit,
            offset = offset,
        });
        return result;
    }

    private async Task ValidateCreateFavoriteRequest(long userId, long assetId)
    {
        // Just make sure the asset actually exists.
        var details = await GetAssetCatalogInfo(assetId);
    }

    public async Task CreateFavorite(long userId, long assetId)
    {
        await ValidateCreateFavoriteRequest(userId, assetId);
        await db.ExecuteAsync(
            "INSERT INTO asset_favorite (user_id, asset_id) VALUES (:user_id, :asset_id) ON CONFLICT (asset_id, user_id) DO NOTHING",
            new
            {
                user_id = userId,
                asset_id = assetId,
            });
    }

    public async Task DeleteFavorite(long userId, long assetId)
    {
        await db.ExecuteAsync("DELETE FROM asset_favorite WHERE user_id = :user_id AND asset_id = :asset_id", new
        {
            user_id = userId,
            asset_id = assetId,
        });
    }

    public async Task InsertAssetModerationLog(long assetId, long actorId, ModerationStatus newStatus)
    {
        await db.ExecuteAsync("INSERT INTO moderation_manage_asset(actor_id, asset_id, action) VALUES (:user_id, :asset_id, :action)", new
        {
            user_id = actorId,
            asset_id = assetId,
            action = newStatus,
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