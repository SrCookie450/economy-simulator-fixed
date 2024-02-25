using System.Diagnostics;
using Dapper;
using RedLockNet;
using Roblox.Dto;
using Roblox.Dto.Trades;
using Roblox.Dto.Users;
using Roblox.Libraries;
using Roblox.Libraries.Exceptions;
using Roblox.Logging;
using Roblox.Models.Economy;
using Roblox.Models.Trades;
using Roblox.Models.Users;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;
using MultiGetEntry = Roblox.Dto.Assets.MultiGetEntry;

namespace Roblox.Services;

public class TradesService : ServiceBase, IService
{
    private async Task<bool> WouldExceedMaxCopyCount(long userId, List<TradeItemEntryDb> receivingItems, List<TradeItemEntryDb> sendingItems)
    {
        // assetId to copy count. can be negative if user is losing items.
        var totals = new Dictionary<long, long>();
        foreach (var item in receivingItems)
        {
            if (!totals.ContainsKey(item.assetId))
                totals[item.assetId] = 0;
            totals[item.assetId]++;
        }

        foreach (var item in sendingItems)
        {
            if (!totals.ContainsKey(item.assetId))
                totals[item.assetId] = 0;
            totals[item.assetId]--;
        }

        using var users = ServiceProvider.GetOrCreate<UsersService>(this);

        foreach (var asset in totals)
        {
            var oldTotal = asset.Value;
            // Quick exit - if user is not gaining items, we don't have to check copy counts
            if (oldTotal < 1)
                continue;
            var ownedCopies = await users.GetUserAssets(userId, asset.Key);
            var newTotal = ownedCopies.Count() + oldTotal;
            // If decreasing copy count, we're OK since user got items after this system was implemented.
            if (newTotal < oldTotal)
                continue;
            // There is a dynamic limit for each item. Make sure user does not exceed that limit.
            var maxPossibleCopies = await users.GetMaximumCopyCount(asset.Key);
            if (newTotal > maxPossibleCopies)
                return true;
        }

        return false;
    }
    
    private async Task<TradeAbuseFailureReason> CanTradeBeCompeted(long tradeId, long senderId, long recipientId)
    {
        // basic heuristics to detect abusive activity.
        // for a similar function, see UsersService.CanAssetBePurchased()
        // list of things we currently try to detect:
        //  - people alt hoarding newly released collectible items
        var timer = new Stopwatch();
        timer.Start();

        using var us = ServiceProvider.GetOrCreate<UsersService>(this);
        // var senderApp = await us.GetApplicationByUserId(senderId);
        // var recipientApp = await us.GetApplicationByUserId(recipientId);

        var senderInvite = await us.GetUserInvite(senderId);
        var recipientInvite = await us.GetUserInvite(recipientId);

        var didRecipientJoinFromSender = recipientInvite?.authorId == senderId;
        var didSenderJoinFromRecipient = senderInvite?.authorId == recipientId;
        var usersInvitedBySamePerson = recipientInvite != null && senderInvite != null && recipientInvite.authorId == senderInvite.authorId;
        var didAnyUserJoinFromInviteByRelatedParty = 
            didRecipientJoinFromSender || 
            didSenderJoinFromRecipient || 
            usersInvitedBySamePerson;
        
        using var assetService = ServiceProvider.GetOrCreate<AssetsService>(this);
        var allItems = (await GetTradeItems(tradeId)).ToList();
        var recipientItems = new List<TradeItemEntryDb>(); // Items the recipient is giving (currently has)
        var senderItems = new List<TradeItemEntryDb>(); // Items the sender is giving (currently has)
        var itemData = new Dictionary<long, MultiGetEntry>();
        foreach (var i in allItems)
        {
            if (!itemData.ContainsKey(i.assetId))
            {
                itemData[i.assetId] = await assetService.GetAssetCatalogInfo(i.assetId);
                if (itemData[i.assetId].isForSale)
                {
                    // If any one item is still for sale, check if this is someone trying to hoard it
                    if (didAnyUserJoinFromInviteByRelatedParty)
                        return TradeAbuseFailureReason.UsersRelatedAndItemStillForSale;
                }
            }

            var userAsset = await us.GetUserAssetById(i.userAssetId);
            var updatedRecently = userAsset.updatedAt > DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(15));
            // check if userId is still equal so we don't give errors for trades that can't be completed anyway.
            if (userAsset.userId == i.userId && updatedRecently && didAnyUserJoinFromInviteByRelatedParty)
                return TradeAbuseFailureReason.UsersRelatedAndUserAssetUpdatedRecently;


            if (i.userId == senderId)
            {
                senderItems.Add(i);
            }
            else
            {
                recipientItems.Add(i);
            }
        }

        // flag for integration tests mainly
        if (FeatureFlags.IsEnabled(FeatureFlag.TradePreventAcceptanceIfTooManyCopies))
        {
            if (await WouldExceedMaxCopyCount(senderId, recipientItems, senderItems) ||
                await WouldExceedMaxCopyCount(recipientId, senderItems, recipientItems))
                return TradeAbuseFailureReason.UserWouldHaveTooManyCopiesIfCompleted;
        }

        timer.Stop();
        Writer.Info(LogGroup.AbuseDetection, "TradeService.CanTradeBeCompeted() tradeId={0} took {1}ms", tradeId, timer.ElapsedMilliseconds);
        return TradeAbuseFailureReason.Ok;
    }
    
    public async Task<int> CountInboundTrades(long userId)
    {
        var t = await db.QuerySingleOrDefaultAsync<Total>(
            "SELECT COUNT(*) AS total FROM user_trade WHERE user_id_two = :id AND status = :status", new
            {
                id = userId,
                status = TradeStatus.Open,
            });
        return t.total;
    }

    public async Task ExpireTrade(long tradeId)
    {
        await db.ExecuteAsync("UPDATE user_trade SET status = :status WHERE status = :open_status AND id = :id", new
        {
            id = tradeId,
            status = TradeStatus.Expired,
            open_status = TradeStatus.Open,
        });
    }

    public async Task<IEnumerable<TradeEntryDb>> GetTradesOfTypeAndExpire(long userId, TradeType type, int limit, int offset)
    {
        var i = 0;
        var trades = Array.Empty<TradeEntryDb>();
        while (i < 10)
        {
            i++;
            trades = (await GetTradesOfType(userId, type, limit, offset)).ToArray();
            var didExpireAnyTrades = false;
            foreach (var trade in trades)
            {
                var isExpired = trade.expiresAt <= DateTime.UtcNow;
                var canBeExpired = trade.status == TradeStatus.Open;
                if (isExpired && canBeExpired)
                {
                    await ExpireTrade(trade.id);
                    didExpireAnyTrades = true;
                }
            }

            if (!didExpireAnyTrades)
                return trades;
        }

        return trades;
    }

    public async Task<IEnumerable<TradeEntryDb>> GetTradesOfType(long userId, TradeType type, int limit, int offset)
    {
        var sql = new SqlBuilder();
        var t = sql.AddTemplate(
            "SELECT u.username as partnerUsername, user_trade.id, user_trade.status, user_trade.created_at as createdAt, user_trade.updated_at as updatedAt, user_trade.expires_at as expiresAt, /**select**/ FROM user_trade /**leftjoin**/ /**where**/ ORDER BY user_trade.id DESC LIMIT :limit OFFSET :offset",
            new
            {
                limit, offset,
            });
        switch (type)
        {
            case TradeType.Inbound:
                sql.Select("user_id_one as partnerId");
                sql.Where("user_id_two = :my_id", new {my_id = userId});
                sql.Where("user_trade.status = :s", new {s = TradeStatus.Open});
                sql.LeftJoin("\"user\" u ON u.id = user_trade.user_id_one");
                break;
            case TradeType.Outbound:
                sql.Select("user_id_two as partnerId");
                sql.Where("user_id_one = :my_id AND user_trade.status = :s",
                    new {my_id = userId, s = TradeStatus.Open});
                sql.LeftJoin("\"user\" u ON u.id = user_trade.user_id_two");
                break;
            case TradeType.Completed:
                sql.Select("CASE WHEN user_id_one = :my_id THEN user_id_two ELSE user_id_one END as partnerId", new
                {
                    my_id = userId,
                });
                sql.OrWhere("user_id_one = :my_id AND user_trade.status = :status", new
                {
                    my_id = userId,
                    status = TradeStatus.Completed,
                });
                sql.OrWhere("user_id_two = :my_id AND user_trade.status = :status", new
                {
                    my_id = userId,
                    status = TradeStatus.Completed,
                });
                sql.LeftJoin(
                    "\"user\" u ON u.id = (CASE WHEN user_id_one = :my_id THEN user_id_two ELSE user_id_one END)", new
                    {
                        my_id = userId,
                    });
                break;
            case TradeType.Inactive:
                sql.Select("CASE WHEN user_id_one = :my_id THEN user_id_two ELSE user_id_one END as partnerId", new
                {
                    my_id = userId,
                });
                sql.OrWhere(
                    "user_id_one = :my_id AND user_trade.status != :status AND user_trade.status != :other_status", new
                    {
                        my_id = userId,
                        status = TradeStatus.Completed,
                        other_status = TradeStatus.Open,
                    });
                sql.OrWhere(
                    "user_id_two = :my_id AND user_trade.status != :status AND user_trade.status != :other_status", new
                    {
                        my_id = userId,
                        status = TradeStatus.Completed,
                        other_status = TradeStatus.Open,
                    });
                sql.LeftJoin(
                    "\"user\" u ON u.id = (CASE WHEN user_id_one = :my_id THEN user_id_two ELSE user_id_one END)", new
                    {
                        my_id = userId,
                    });
                break;
            default:
                throw new ArgumentException("Unexpected "+nameof(TradeStatus)+": " + type);
        }

        return await db.QueryAsync<TradeEntryDb>(t.RawSql, t.Parameters);
    }

    public async Task<TradeEntryDbFull> GetTradeById(long tradeId)
    {
        var tradeData = await db.QuerySingleOrDefaultAsync<TradeEntryDbFull>(
            "SELECT user_trade.id, user_trade.user_id_one as userIdOne, t1.username as usernameOne, user_trade.user_id_two as userIdTwo, t2.username as usernameTwo, user_id_one_robux as userOneRobux, user_id_two_robux as userTwoRobux, user_trade.created_at as createdAt, user_trade.updated_at as updatedAt, user_trade.expires_at as expiresAt, user_trade.status FROM user_trade INNER JOIN \"user\" t1 ON t1.id = user_trade.user_id_one INNER JOIN \"user\" t2 ON t2.id = user_trade.user_id_two WHERE user_trade.id = :id",
            new
            {
                id = tradeId,
            });

        if (tradeData == null) throw new RecordNotFoundException();
        return tradeData;
    }

    public async Task<IEnumerable<TradeItemEntryDb>> GetTradeItems(long tradeId)
    {
        return await db.QueryAsync<TradeItemEntryDb>(
            "SELECT trade_id as id, user_trade_asset.user_id as userId, user_asset_id as userAssetId, asset.recent_average_price as recentAveragePrice, user_asset.asset_id as assetId, user_asset.serial, asset.price_robux as price, asset.serial_count as serialCount, asset.name FROM user_trade_asset INNER JOIN user_asset ON user_Asset.id = user_trade_asset.user_asset_id INNER JOIN asset ON asset.id = user_asset.asset_id WHERE trade_id = :id",
            new
            {
                id = tradeId,
            });
    }

    private Task<IRedLock> GetLockForTrade(long tradeId)
    {
        return Cache.redLock.CreateLockAsync("TradeV1:" + tradeId, TimeSpan.FromMinutes(2));
    }

    private async Task<bool> CanTradeMembershipCheck(long userId)
    {
        using var us = ServiceProvider.GetOrCreate<UsersService>();
        var premiumStatus = await us.GetUserMembership(userId);
        return premiumStatus != null && premiumStatus.membershipType != MembershipType.None;
    }
    
    private async Task<bool> CanOneTradeWithTwo(long userIdOne, long userIdTwo)
    {
        using var accountInfo = ServiceProvider.GetOrCreate<AccountInformationService>();
        // check first
        var privacy = await accountInfo.GetUserTradePrivacy(userIdOne);
        if (privacy == GeneralPrivacy.NoOne)
            return false;
        if (privacy == GeneralPrivacy.All)
            return true;
        
        using var friendsService = ServiceProvider.GetOrCreate<FriendsService>();
        // Check friendship first since that overrides follows/followings
        if (privacy is GeneralPrivacy.Followers or GeneralPrivacy.Following or GeneralPrivacy.Friends)
        {
            var friendshipStatus = await friendsService.GetFriendshipStatus(userIdOne, userIdTwo);
            if (friendshipStatus.status == "Friends")
                return true;
        }

        if (privacy is GeneralPrivacy.Following)
        {
            if (await friendsService.IsOneFollowingTwo(userIdTwo, userIdOne))
                return true;
        }
        
        if (privacy is GeneralPrivacy.Followers)
        {
            if (await friendsService.IsOneFollowingTwo(userIdOne, userIdTwo))
                return true;
        }

        // probably shouldn't be hit? not sure yet...
        return false;
    }

    public async Task<bool> CanTrade(long userIdOne, long userIdTwo)
    {
        // user can't trade with themself!
        if (userIdOne == userIdTwo) return false;
        // check privacy settings
        var ok = await CanOneTradeWithTwo(userIdOne, userIdTwo);
        if (!ok) return false;
        ok = await CanOneTradeWithTwo(userIdTwo, userIdOne);
        if (!ok) return false;
        // confirm users are not banned
        var banStatusOne = await db.QuerySingleOrDefaultAsync<AccountStatusEntry>(
            "SELECT status FROM \"user\" WHERE id = :id", new
            {
                id = userIdOne,
            });
        if (banStatusOne.status != AccountStatus.Ok) return false;
        var banStatusTwo = await db.QuerySingleOrDefaultAsync<AccountStatusEntry>(
            "SELECT status FROM \"user\" WHERE id = :id", new
            {
                id = userIdTwo,
            });
        if (banStatusTwo.status != AccountStatus.Ok) return false;


        return true;
    }

    public async Task<bool> IsUserInFloodCheck(long userId, long otherUserId)
    {
        // You can send up to 15 trades every 5 minutes
        // There can be up to 5 pending trades send from userId to otherUserId at any given time
        var latestTrades = await db.QuerySingleOrDefaultAsync<Total>(
            "SELECT COUNT(*) AS total FROM user_trade WHERE created_at >= :dt AND user_id_one = :id", new
            {
                id = userId,
                dt = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5)),
            });
        if (latestTrades.total >= 15) return true;
        var pendingTradesWithOtherUser = await db.QuerySingleOrDefaultAsync<Total>(
            "SELECT COUNT(*) as total FROM user_trade WHERE user_id_one = :one AND user_id_two = :two AND status = :st",
            new
            {
                one = userId,
                two = otherUserId,
                st = (int) TradeStatus.Open,
            });
        if (pendingTradesWithOtherUser.total >= 5) return true;
        return false;
    }

    public async Task<TradeQualityFilter> GetFilter(long userId)
    {
        var result = await db.QuerySingleOrDefaultAsync<TradeFilterEntry>(
            "SELECT trade_filter as filter FROM user_settings WHERE user_id = :id", new
            {
                id = userId,
            });
        return result.filter;
    }

    public async Task<IEnumerable<ConfirmOwnershipEntry>> MultiConfirmOwnership(long userId,
        IEnumerable<long> userAssetIds)
    {
        var userAssets = userAssetIds.ToList();
        var sql = new SqlBuilder();
        var t = sql.AddTemplate(
            "SELECT user_asset.asset_id as assetId, user_asset.id as userAssetId, asset.is_limited as isLimited, asset.is_limited_unique as isLimitedUnique, user_asset.user_id as userId, asset.recent_average_price as recentAveragePrice, asset.is_for_sale as isForSale FROM user_asset INNER JOIN asset ON asset.id = user_asset.asset_id /**where**/");
        sql.OrWhereMulti("user_asset.id = $1", userAssets);
        var result = await db.QueryAsync<ConfirmOwnershipEntry>(t.RawSql, t.Parameters);
        var response = result.Select(c =>
        {
            c.isOwner = c.userId == userId;
            return c;
        }).ToList();
        if (response.Count != userAssets.Count)
            throw new ArgumentException("One or more of the userAssetIds specified are invalid");
        return response;
    }

    public enum SendTradeErrorCodes
    {
        Generic = 0,
        CannotTrade = 7,
        NoOffers = 8,
        TooLittleRobux = 11,
        BadUserAssets = 12,
        FloodCheck = 14,
        BadTradeRatio = 16,
        InsufficientRobux = 17,
        TooMuchRobux = 18,
        CannotTradeWithSelf = 21,
    }

    public Dictionary<SendTradeErrorCodes, string> errorMessages = new()
    {
        {SendTradeErrorCodes.Generic, "BadRequest"},
        {SendTradeErrorCodes.NoOffers, "The trade request should include offers"},
        {SendTradeErrorCodes.CannotTradeWithSelf, "Cannot trade with yourself"},
        {SendTradeErrorCodes.TooMuchRobux, "Too many Robux in one side of the offer"},
        {SendTradeErrorCodes.TooLittleRobux, "Cannot add negative Robux amounts to a trade"},
        {SendTradeErrorCodes.CannotTrade, "The user cannot trade"},
        {SendTradeErrorCodes.InsufficientRobux, "You have insufficient Robux to make this offer"},
        {
            SendTradeErrorCodes.FloodCheck,
            "You are sending too many trade requests. Please slow down and try again later."
        },
        {SendTradeErrorCodes.BadUserAssets, "One or more userAssets are invalid"},
        {SendTradeErrorCodes.BadTradeRatio, "Trade value ratio is not sufficient"},
    };

    public async Task SendTrade(long userIdSender, IEnumerable<CreateTradeOffer> offers, bool sendMessage)
    {
        var log = Writer.CreateWithId(LogGroup.TradeSend);
        log.Info("SendTrade starting. senderId = {0} sendMessage = {1}", userIdSender, sendMessage);
        var logic = new Logic<SendTradeErrorCodes>(errorMessages);
        var offerList = offers.ToList();
        var offer = offerList.Find(c => c.userId == userIdSender);
        var request = offerList.Find(c => c.userId != userIdSender);
        log.Info("Request userId = {0}", request?.userId);
        // checks
        logic.Requires(SendTradeErrorCodes.NoOffers, offer != null && request != null);
        Debug.Assert(offer != null && request != null);
        var offerUserAssets = offer.userAssetIds.Distinct().ToList();
        var requestUserAssets = request.userAssetIds.Distinct().ToList();
        log.Info("Offer len = {0} request len = {1}", offerUserAssets.Count, requestUserAssets.Count);

        // Make sure there are offers
        logic.Requires(SendTradeErrorCodes.NoOffers, offerUserAssets.Count >= 1 && requestUserAssets.Count >= 1);

        // Only allowed up to 4 items
        logic.Requires(SendTradeErrorCodes.Generic, offerUserAssets.Count <= 4 && requestUserAssets.Count <= 4);

        var globalMaxRobux = 10000000; // 10 m
        logic.Requires(SendTradeErrorCodes.TooMuchRobux, (offer.robux == null || offer.robux < globalMaxRobux) &&
                                                         (request.robux == null || request.robux < globalMaxRobux));
        // Confirm offering over 0 robux, if not null
        logic.Requires(SendTradeErrorCodes.TooLittleRobux, offer.robux is null or > 0);
        logic.Requires(SendTradeErrorCodes.TooLittleRobux, request.robux is null or > 0);
        // check premium status
        if (!await CanTradeMembershipCheck(offer.userId))
            throw new ArgumentException("Offer user must have builders club to trade");
        if (!await CanTradeMembershipCheck(request.userId))
            throw new ArgumentException("Request user must have builders club to trade");
        // Check if both parties can trade with eachother
        var canTrade = await CanTrade(offer.userId, request.userId);
        logic.Requires(SendTradeErrorCodes.CannotTrade, canTrade);
        // if applicable, confirm requester has enough robux
        if (offer.robux != null)
        {
            log.Info("offering {0} robux",offer.robux);
            var userRobux = await db.QuerySingleOrDefaultAsync<UserEconomy>(
                "SELECT balance_robux as robux FROM user_economy WHERE user_id = :id", new
                {
                    id = offer.userId,
                });
            logic.Requires(SendTradeErrorCodes.InsufficientRobux, userRobux.robux >= offer.robux);
            log.Info("has enough robux to make this offer");
        }

        // make sure user isn't in the cooldown
        var isInCooldown = await IsUserInFloodCheck(userIdSender, request.userId);
        logic.Requires(SendTradeErrorCodes.FloodCheck, FailType.FloodCheck, !isInCooldown);
        // high = offer must be 75% or more of rap of request
        // med = offer must be 50% or more of rap of request
        // low = offer must be 25% or more of rap of request
        // none = offer and request can be anything
        var filter = await GetFilter(request.userId);
        log.Info("recipient filter = {0}", filter);
        await InTransaction(async _ =>
        {
            var offeredItems = (await MultiConfirmOwnership(offer.userId, offerUserAssets)).ToList();
            var requestedItems = (await MultiConfirmOwnership(request.userId, requestUserAssets)).ToList();
            // Confirm that users own the items offered/requested, all are limited, and that all are not for sale
            logic.Requires(SendTradeErrorCodes.BadUserAssets,
                offeredItems.Find(v => !v.isOwner || (!v.isLimited && !v.isLimitedUnique) /*|| v.isForSale*/) == null);
            logic.Requires(SendTradeErrorCodes.BadUserAssets,
                requestedItems.Find(v => !v.isOwner || (!v.isLimited && !v.isLimitedUnique) /*|| v.isForSale*/) == null);

            var totalRequestedRap = requestedItems.Select(c => c.recentAveragePrice ?? 0).Sum();
            var totalOfferedRap = offeredItems.Select(c => c.recentAveragePrice ?? 0).Sum();
            if (filter != TradeQualityFilter.None)
            {
                var minimumOfferRapPercent = filter == TradeQualityFilter.Low ? 25 :
                    filter == TradeQualityFilter.Medium ? 50 :
                    filter == TradeQualityFilter.High ? 75 : 1;
                var percentAsDecimal = (decimal) minimumOfferRapPercent / 100;
                var percentOfRequest = totalRequestedRap * percentAsDecimal;
                log.Info("recipient filter requires offer to be at least {0} - it is {1}", percentOfRequest, totalOfferedRap);

                logic.Requires(SendTradeErrorCodes.BadTradeRatio, totalOfferedRap >= percentOfRequest);
            }

            if (request.robux != null)
            {
                log.Info("requesting {0} robux", request.robux);
                var maxRobux = totalRequestedRap * 0.5;
                logic.Requires(SendTradeErrorCodes.TooMuchRobux, request.robux <= maxRobux);
            }

            if (offer.robux != null)
            {
                log.Info("offering {0} robux",offer.robux);
                var maxRobux = totalOfferedRap * 0.5;
                logic.Requires(SendTradeErrorCodes.TooMuchRobux, offer.robux <= maxRobux);
            }

            var expirationDate = DateTime.UtcNow.Add(TimeSpan.FromDays(5));
            var tradeId = await InsertAsync("user_trade", new
            {
                user_id_one = offer.userId,
                user_id_two = request.userId,
                user_id_one_robux = offer.robux,
                user_id_two_robux = request.robux,
                status = TradeStatus.Open,
                expires_at = expirationDate,
            });
            log.Info("insert trade. id = {0} expiration = {1}", tradeId, expirationDate);
            // add offer items
            foreach (var id in offerUserAssets)
            {
                log.Info("offering userAssetId {0}", id);
                await InsertAsync("user_trade_asset", "trade_id", new
                {
                    trade_id = tradeId,
                    user_id = offer.userId,
                    user_asset_id = id,
                });
            }

            // add request items
            foreach (var id in requestUserAssets)
            {
                log.Info("requesting userAssetId = {0}", id);
                await InsertAsync("user_trade_asset", "trade_id", new
                {
                    trade_id = tradeId,
                    user_id = request.userId,
                    user_asset_id = id,
                });
            }

            if (sendMessage)
            {
                log.Info("sending message to recipient");
                var userDetails = await db.QuerySingleOrDefaultAsync("SELECT username FROM \"user\" WHERE id = :id",
                    new {id = offer.userId});
                await InsertAsync("user_message", new
                {
                    user_id_to = request.userId,
                    user_id_from = 1,
                    subject = $"You have a Trade request from {(string) userDetails.username}",
                    body = "You have a new trade! Go to the Trades tab to review your trades.",
                    is_read = false,
                    is_archived = false,
                });
            }
            
            log.Info("trade sent successfully");

            return 0;
        });
    }

    public async Task AcceptTrade(long tradeId, long contextUserId)
    {
        var log = Writer.CreateWithId(LogGroup.TradeAccept);
        log.Info("AcceptTrade starting. tradeId = {0} contextUserId = {1}", tradeId, contextUserId);
        var logic = new Logic<SendTradeErrorCodes>(errorMessages);
        await using var tradeLock = await GetLockForTrade(tradeId);
        if (!tradeLock.IsAcquired) throw new LockNotAcquiredException();

        await InTransaction(async _ =>
        {
            var info = await GetTradeById(tradeId);
            if (info.userIdTwo != contextUserId || info.status != TradeStatus.Open)
                throw new ArgumentException("User is not authorized to modify this trade");
            // mark as pending
            log.Info("update {0} to pending", tradeId);
            await db.ExecuteAsync("UPDATE user_trade SET status = :status WHERE id = :id", new
            {
                id = tradeId,
                status = TradeStatus.Pending,
            });
            var offerUserId = info.userIdOne;
            var requestUserId = info.userIdTwo;
            var offerRobux = info.userOneRobux;
            var requestRobux = info.userTwoRobux;
            log.Info("offerUserId = {0} requestUserId = {1} offerRobux = {2} requestRobux = {3}", offerUserId, requestUserId, offerRobux, requestRobux);
            // check status
            using var us = ServiceProvider.GetOrCreate<UsersService>(this);
            if (!await us.IsUserStaff(offerUserId))
            {
                var canBeCompleted = await CanTradeBeCompeted(tradeId, offerUserId, requestUserId);
                if (canBeCompleted != TradeAbuseFailureReason.Ok)
                {
                    log.Info("trade cannot be completed - CanTradeBeCompeted failed with {0}",
                        canBeCompleted.ToString());
                    throw new ArgumentException("Cannot accept this trade");
                }
            }

            if (offerRobux is < 0)
                throw new ArgumentException("Cannot accept this trade - it has negative offerRobux");
            if (requestRobux is < 0)
                throw new ArgumentException("Cannot accept this trade - it has negative requestRobux");
            // check premium status
            if (!await CanTradeMembershipCheck(offerUserId))
                throw new ArgumentException("Offer user must have builders club to trade");
            if (!await CanTradeMembershipCheck(requestUserId))
                throw new ArgumentException("Request user must have builders club to trade");
            // Confirm users can still trade
            var canTrade = await CanTrade(offerUserId, requestUserId);
            log.Info("can offer {0} trade with requester {1}: {2}", offerUserId, requestUserId, canTrade);
            logic.Requires(SendTradeErrorCodes.CannotTrade, canTrade);
            // List of locks acquired for robux changes. Only added if necessary
            await using var economyDisposable = new CombinedAsyncDisposable();
            using var ec = ServiceProvider.GetOrCreate<EconomyService>(this);

            // Confirm users still have Robux:
            // Offer
            if (offerRobux != null)
            {
                economyDisposable.AddChild(await us.AcquireEconomyLock(offerUserId));
                log.Info("offerRobux = {0}", offerRobux);
                var bal = await ec.GetUserRobux(offerUserId);
                logic.Requires(SendTradeErrorCodes.Generic, bal >= offerRobux);
                log.Info("user offering has enough robux");
            }

            // Request
            if (requestRobux != null)
            {
                economyDisposable.AddChild(await us.AcquireEconomyLock(requestUserId));
                log.Info("requestRobux = {0}", requestRobux);
                var bal = await ec.GetUserRobux(requestUserId);
                logic.Requires(SendTradeErrorCodes.Generic, bal >= requestRobux);
                log.Info("user requesting has enough robux");
            }
            // confirm ownership
            var assets = (await GetTradeItems(tradeId)).ToList();
            var offer = assets.Where(c => c.userId == offerUserId).ToList();
            var request = assets.Where(c => c.userId == requestUserId).ToList();
            // get user asset ids list - also confirm that there are no duplicates
            var userAssetIds = assets.Select(c => c.userAssetId).Distinct().ToList();
            if (userAssetIds.Count != assets.Count)
                throw new ArgumentException(
                    "Cannot accept this trade - distinct userAssetIds is different length from assets list");
            
            // we need to get a userAsset lock
            var lockStart = new Stopwatch();
            lockStart.Start();
            await using var userAssetsLock = await us.MultiAcquireUserAssetLock(userAssetIds);
            lockStart.Stop();
            log.Info("It took {0}ms to acquire UserAsset lock", lockStart.ElapsedMilliseconds);
            
            var offerAssets = (await MultiConfirmOwnership(offerUserId, offer.Select(c => c.userAssetId))).ToList();
            var requestedAssets =
                (await MultiConfirmOwnership(requestUserId, request.Select(c => c.userAssetId))).ToList();

            // Confirm items are still owned by correct parties
            logic.Requires(SendTradeErrorCodes.BadUserAssets, offerAssets.Find(v => !v.isOwner) == null);
            logic.Requires(SendTradeErrorCodes.BadUserAssets, requestedAssets.Find(v => !v.isOwner) == null);

            // Update user assets
            // Give offer items to requester (aka accepter)
            foreach (var item in offerAssets)
            {
                log.Info("transferring offer item {0} from {1} to {2}", item.userAssetId, item.userId, requestUserId);
                await db.ExecuteAsync("UPDATE user_asset SET user_id = :user_id, price = 0, updated_at = now() WHERE id = :id", new
                {
                    user_id = requestUserId,
                    id = item.userAssetId,
                });
            }
            // Give request items to offerer
            foreach (var item in requestedAssets)
            {
                log.Info("transferring request item {0} from {1} to {2}", item.userAssetId, item.userId, offerUserId);
                await db.ExecuteAsync("UPDATE user_asset SET user_id = :user_id, price = 0, updated_at = now() WHERE id = :id", new
                {
                    user_id = offerUserId,
                    id = item.userAssetId,
                });
            }

            if (requestRobux != null)
            {
                // TODO: We need to add transactions here
                // Robux that request is giving to offer person
                // Deduct
                log.Info("subtract request robux {0} from {1}", requestRobux.Value, requestUserId);
                await ec.DecrementCurrency(requestUserId, CurrencyType.Robux, requestRobux.Value);
                // Give 70% to other user
                var percentToOtherUser = (long)Math.Truncate((decimal) (requestRobux * 0.7));
                log.Info("transferring request robux {0} to {1}", percentToOtherUser, offerUserId);
                await ec.IncrementCurrency(offerUserId, CurrencyType.Robux, percentToOtherUser);
            }
            
            if (offerRobux != null)
            {
                // TODO: We need to add transactions here
                // Robux that offer is giving to request person
                // Deduct
                log.Info("subtract offer robux {0} from {1}", offerRobux, offerUserId);
                await ec.DecrementCurrency(offerUserId, CurrencyType.Robux, offerRobux.Value);
                // Give 70% to other user
                var percentToOtherUser = (long)Math.Truncate((decimal) (offerRobux * 0.7));
                log.Info("transferring offer robux {0} to {1}", percentToOtherUser, requestUserId);
                await ec.IncrementCurrency(requestUserId, CurrencyType.Robux, percentToOtherUser);
            }
            
            await db.ExecuteAsync("UPDATE user_trade SET status = :status WHERE id = :id", new
            {
                id = tradeId,
                status = TradeStatus.Completed,
            });
            log.Info("updated trade status to {0}. success", TradeStatus.Completed);

            return 0;
        });
    }

    public async Task DeclineTrade(long tradeId, long contextUserId)
    {
        await using var tradeLock = await GetLockForTrade(tradeId);
        if (!tradeLock.IsAcquired) throw new LockNotAcquiredException();

        var info = await GetTradeById(tradeId);
        if (info.userIdOne != contextUserId && info.userIdTwo != contextUserId)
            throw new ArgumentException("User is not authorized to modify this trade");

        if (info.status != TradeStatus.Open)
            throw new ArgumentException("Trade with status " + info.status + " cannot be declined");

        await InTransaction(async _ =>
        {
            // userIdOne is the sender. If sender is not the one cancelling, inform sender the trade was declined.
            if (info.userIdOne != contextUserId)
            {
                await InsertAsync("user_message", new
                {
                    user_id_to = info.userIdOne,
                    user_id_from = 1,
                    subject = $"Your trade with {info.usernameTwo} has ended.",
                    body =
                        "Trade declined. If you wish to review this trade, go to the Trades tab and select 'inactive'.",
                    is_read = false,
                    is_archived = false,
                });
            }

            // decline
            await db.ExecuteAsync("UPDATE user_trade SET status = :status WHERE id = :id", new
            {
                id = tradeId,
                status = TradeStatus.Declined,
            });

            return 0;
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