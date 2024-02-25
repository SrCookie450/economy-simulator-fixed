
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Roblox.Dto.Trades;
using Roblox.Libraries.Exceptions;
using Roblox.Models.Trades;
using Roblox.Models.Users;
using Roblox.Services;
using Roblox.Services.IntegrationTest;
using Roblox.Website.Controllers;
using Xunit;
using Xunit.Abstractions;

public class TradeItem
{
    public long userAssetId { get; set; }
    public long assetId { get; set; }
}

public class TradeTest : TestBase
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TradeTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private async Task SetRap(long assetId, long rap)
    {
        await Database.connection.ExecuteAsync("UPDATE asset SET recent_average_price = :rap WHERE id = :id", new
        {
            rap = rap,
            id = assetId,
        });
    }

    private async Task<List<TradeItem>> CreateItemsForTrade(long userId, int count)
    {
        Debug.Assert(count <= 4 && count > 0);
        var roblox = await CreateRandomUser();
        var asset = await CreateRandomItem(roblox);
        // mark item as limited and not for sale so it can be traded
        using (var assets = ServiceProvider.GetOrCreate<AssetsService>())
        {
            await assets.UpdateAssetMarketInfo(asset, false, true, false, null, null);
        }

        var result = new List<TradeItem>();
        using var us = ServiceProvider.GetOrCreate<UsersService>();
        for (var i = 0; i < count; i++)
        {
            var item = await us.CreateUserAsset(userId, asset);
            result.Add(new()
            {
                assetId = asset,
                userAssetId = item,
            });
        }

        return result;
    }

    private async Task SetCanTrade(long userId)
    {
        using var ai = ServiceProvider.GetOrCreate<AccountInformationService>();
        await ai.SetUserTradePrivacy(userId, GeneralPrivacy.All);
    }

    private async Task ConfirmOwnership(long userIdSender, long userIdReceiver, List<TradeItem> senderItems,
        List<TradeItem> receiverItems)
    {
        using var us = ServiceProvider.GetOrCreate<UsersService>();
        foreach (var item in senderItems)
        {
            // receiver must own these now
            var owner = await us.GetUserAssetById(item.userAssetId);
            Assert.Equal(userIdReceiver, owner.userId);
        }

        foreach (var item in receiverItems)
        {
            var owner = await us.GetUserAssetById(item.userAssetId);
            // sender must own these
            Assert.Equal(userIdSender, owner.userId);
        }
    }
    
    

    [Fact]
    public async Task Create_One_Trade_Success()
    {
        var sender = await CreateRandomUser();
        var receiver = await CreateRandomUser();

        using var us = ServiceProvider.GetOrCreate<UsersService>();
        using var ec = ServiceProvider.GetOrCreate<EconomyService>();
        
        var senderBalance = await ec.GetUserRobux(sender);
        var receiverBalance = await ec.GetUserRobux(receiver);

        // make users able to trade
        await SetCanTrade(sender);
        await SetCanTrade(receiver);
        
        var senderItems = await CreateItemsForTrade(sender, 2);
        var receiverItems = await CreateItemsForTrade(receiver, 4);

        using var ts = ServiceProvider.GetOrCreate<TradesService>();
        // create le trade
        var offers = new List<CreateTradeOffer>()
        {
            new()
            {
                robux = null,
                userAssetIds = senderItems.Select(c => c.userAssetId),
                userId = sender,
            },
            new()
            {
                robux = null,
                userAssetIds = receiverItems.Select(c => c.userAssetId),
                userId = receiver,
            },
        };
        var watch = new Stopwatch();
        watch.Start();
        await ts.SendTrade(sender, offers, true);
        watch.Stop();
        Assert.True(watch.ElapsedMilliseconds < 100);
        _testOutputHelper.WriteLine("SendTrade time = {0}ms",watch.ElapsedMilliseconds);
        // get id
        var inbound = await ts.GetTradesOfType(receiver, TradeType.Inbound, 1, 0);
        var trades = inbound as TradeEntryDb[] ?? inbound.ToArray();
        Assert.Single(trades);
        Assert.True(trades[0].id != 0);
        var tradeId = trades[0].id;
        // confirm user got message
        using var pm = ServiceProvider.GetOrCreate<PrivateMessagesService>();
        // var messages = (await pm.GetMessages(receiver, "inbox", 100, 0)).collection.ToArray();
        // Assert.StartsWith("You have a Trade request from", (string) messages[0].subject);
        // confirm it shows up in senders outbound
        var outbound = (await ts.GetTradesOfType(sender, TradeType.Outbound, 1, 0)).ToArray();
        Assert.Single(outbound);
        Assert.Equal(tradeId, outbound[0].id);
        // try to accept it
        watch.Restart();
        await ts.AcceptTrade(tradeId, receiver);
        watch.Stop();
        Assert.True(watch.ElapsedMilliseconds < 100);
        _testOutputHelper.WriteLine("AcceptTrade time = {0}ms",watch.ElapsedMilliseconds);
        // check for ownership
        await ConfirmOwnership(sender, receiver, senderItems, receiverItems);
        // confirm balances weren't changed
        var newSenderBalance = await ec.GetUserRobux(sender);
        Assert.Equal(senderBalance, newSenderBalance); // We didn't change this!
        var newReceiverBalance = await ec.GetUserRobux(receiver);
        Assert.Equal(receiverBalance, newReceiverBalance); // We didn't change this either!
        // confirm message was sent to sender
        // we don't do this yet :(
        /*
        messages = (await pm.GetMessages(sender, "inbox", 100, 0)).collection.ToArray();
        Assert.Single(messages);
        Assert.StartsWith("Trade completed", (string) messages[0].subject);
        */
        // confirm trade history is correct
        var completedSender = (await ts.GetTradesOfType(sender, TradeType.Completed, 100, 0)).ToArray();
        var completedReceiver = (await ts.GetTradesOfType(receiver, TradeType.Completed, 100, 0)).ToArray();
        Assert.Single(completedSender);
        Assert.Single(completedReceiver);
        Assert.Equal(tradeId, completedSender[0].id);
        Assert.Equal(tradeId, completedReceiver[0].id);
        Assert.Equal(receiver, completedSender[0].partnerId);
        Assert.Equal(sender, completedReceiver[0].partnerId);
    }

    [Fact]
    public async Task Create_One_Trade_Success_WithRobux()
    {
        var sender = await CreateRandomUser();
        var receiver = await CreateRandomUser();

        using var us = ServiceProvider.GetOrCreate<UsersService>();
        using var ec = ServiceProvider.GetOrCreate<EconomyService>();
        var senderBalance = await ec.GetUserRobux(sender);
        var receiverBalance = await ec.GetUserRobux(receiver);

        // make users able to trade
        await SetCanTrade(sender);
        await SetCanTrade(receiver);
        
        var senderItems = await CreateItemsForTrade(sender, 2);
        var receiverItems = await CreateItemsForTrade(receiver, 4);

        // sender needs rap for robux offer
        await SetRap(senderItems[0].assetId, 100);
        // buyer does too
        await SetRap(receiverItems[0].assetId, 100);

        var senderRobux = 25;
        var senderRobuxAfterFee = (long) Math.Truncate(senderRobux * 0.7);
        
        using var ts = ServiceProvider.GetOrCreate<TradesService>();
        // create le trade
        var offers = new List<CreateTradeOffer>()
        {
            new()
            {
                robux = senderRobux,
                userAssetIds = senderItems.Select(c => c.userAssetId),
                userId = sender,
            },
            new()
            {
                robux = null,
                userAssetIds = receiverItems.Select(c => c.userAssetId),
                userId = receiver,
            },
        };
        var watch = new Stopwatch();
        watch.Start();
        await ts.SendTrade(sender, offers, true);
        watch.Stop();
        Assert.True(watch.ElapsedMilliseconds < 100);
        _testOutputHelper.WriteLine("SendTrade time = {0}ms",watch.ElapsedMilliseconds);
        // get id
        var inbound = await ts.GetTradesOfType(receiver, TradeType.Inbound, 1, 0);
        var trades = inbound as TradeEntryDb[] ?? inbound.ToArray();
        Assert.Single(trades);
        Assert.True(trades[0].id != 0);
        var tradeId = trades[0].id;
        // confirm user got message
        using var pm = ServiceProvider.GetOrCreate<PrivateMessagesService>();
        // var messages = (await pm.GetMessages(receiver, "inbox", 100, 0)).collection.ToArray();
        // Assert.StartsWith("You have a Trade request from", (string) messages[0].subject);
        // confirm it shows up in senders outbound
        var outbound = (await ts.GetTradesOfType(sender, TradeType.Outbound, 1, 0)).ToArray();
        Assert.Single(outbound);
        Assert.Equal(tradeId, outbound[0].id);
        // try to accept it
        watch.Restart();
        await ts.AcceptTrade(tradeId, receiver);
        watch.Stop();
        Assert.True(watch.ElapsedMilliseconds < 100);
        _testOutputHelper.WriteLine("AcceptTrade time = {0}ms",watch.ElapsedMilliseconds);
        // check for ownership
        await ConfirmOwnership(sender, receiver, senderItems, receiverItems);
        // confirm balances weren't changed
        var newSenderBalance = await ec.GetUserRobux(sender);
        Assert.Equal(senderBalance - senderRobux, newSenderBalance);
        var newReceiverBalance = await ec.GetUserRobux(receiver);
        Assert.Equal(receiverBalance + senderRobuxAfterFee, newReceiverBalance);
        // confirm message was sent to sender
        // we don't do this yet :(
        /*
        messages = (await pm.GetMessages(sender, "inbox", 100, 0)).collection.ToArray();
        Assert.Single(messages);
        Assert.StartsWith("Trade completed", (string) messages[0].subject);
        */
        // confirm trade history is correct
        var completedSender = (await ts.GetTradesOfType(sender, TradeType.Completed, 100, 0)).ToArray();
        var completedReceiver = (await ts.GetTradesOfType(receiver, TradeType.Completed, 100, 0)).ToArray();
        Assert.Single(completedSender);
        Assert.Single(completedReceiver);
        Assert.Equal(tradeId, completedSender[0].id);
        Assert.Equal(tradeId, completedReceiver[0].id);
        Assert.Equal(receiver, completedSender[0].partnerId);
        Assert.Equal(sender, completedReceiver[0].partnerId);
    }
    
    [Fact]
    public async Task Create_One_Trade_Fail_SenderDoesNotOwnItems_BeforeSending()
    {
        var sender = await CreateRandomUser();
        var receiver = await CreateRandomUser();
        var thirdGuy = await CreateRandomUser();
        // make users able to trade
        await SetCanTrade(sender);
        await SetCanTrade(receiver);
                
        var senderItems = await CreateItemsForTrade(sender, 2);
        var receiverItems = await CreateItemsForTrade(receiver, 4);
        // Move a sender item to third guy
        await Database.connection.ExecuteAsync("UPDATE user_asset SET user_id = :user_id WHERE id = :id", new
        {
            user_id = thirdGuy,
            id = senderItems[0].userAssetId,
        });
        using var ts = ServiceProvider.GetOrCreate<TradesService>();
        // create le trade
        var offers = new List<CreateTradeOffer>()
        {
            new()
            {
                robux = null,
                userAssetIds = senderItems.Select(c => c.userAssetId),
                userId = sender,
            },
            new()
            {
                robux = null,
                userAssetIds = receiverItems.Select(c => c.userAssetId),
                userId = receiver,
            },
        };
        var err = await Assert.ThrowsAsync<LogicException>(async () =>
        {
            await ts.SendTrade(sender, offers, true);
        });
        Assert.Equal((int)TradesService.SendTradeErrorCodes.BadUserAssets, err.errorCode);
    }
    
    [Fact]
    public async Task Create_One_Trade_Fail_SenderDoesNotOwnItems_AfterSending()
    {
        var sender = await CreateRandomUser();
        var receiver = await CreateRandomUser();
        var thirdGuy = await CreateRandomUser();
        // make users able to trade
        await SetCanTrade(sender);
        await SetCanTrade(receiver);
                
        var senderItems = await CreateItemsForTrade(sender, 2);
        var receiverItems = await CreateItemsForTrade(receiver, 4);

        using var ts = ServiceProvider.GetOrCreate<TradesService>();
        // create le trade
        var offers = new List<CreateTradeOffer>()
        {
            new()
            {
                robux = null,
                userAssetIds = senderItems.Select(c => c.userAssetId),
                userId = sender,
            },
            new()
            {
                robux = null,
                userAssetIds = receiverItems.Select(c => c.userAssetId),
                userId = receiver,
            },
        };
        await ts.SendTrade(sender, offers, true);
        // Move a sender item to third guy
        await Database.connection.ExecuteAsync("UPDATE user_asset SET user_id = :user_id WHERE id = :id", new
        {
            user_id = thirdGuy,
            id = senderItems[0].userAssetId,
        });
        // get id
        var inbound = await ts.GetTradesOfType(receiver, TradeType.Inbound, 1, 0);
        var trades = inbound as TradeEntryDb[] ?? inbound.ToArray();
        Assert.Single(trades);
        Assert.True(trades[0].id != 0);
        var tradeId = trades[0].id;
        var err = await Assert.ThrowsAsync<LogicException>(async () =>
        {
            await ts.AcceptTrade(tradeId, receiver);
        });
        Assert.Equal((int)TradesService.SendTradeErrorCodes.BadUserAssets, err.errorCode);
    }
    
    [Fact]
    public async Task Create_One_Trade_Fail_RecipientDoesNotOwnItems_BeforeSending()
    {
        var sender = await CreateRandomUser();
        var receiver = await CreateRandomUser();
        var thirdGuy = await CreateRandomUser();
        // make users able to trade
        await SetCanTrade(sender);
        await SetCanTrade(receiver);
                
        var senderItems = await CreateItemsForTrade(sender, 2);
        var receiverItems = await CreateItemsForTrade(receiver, 4);
        // Move a sender item to third guy
        await Database.connection.ExecuteAsync("UPDATE user_asset SET user_id = :user_id WHERE id = :id", new
        {
            user_id = thirdGuy,
            id = receiverItems[0].userAssetId,
        });
        using var ts = ServiceProvider.GetOrCreate<TradesService>();
        // create le trade
        var offers = new List<CreateTradeOffer>()
        {
            new()
            {
                robux = null,
                userAssetIds = senderItems.Select(c => c.userAssetId),
                userId = sender,
            },
            new()
            {
                robux = null,
                userAssetIds = receiverItems.Select(c => c.userAssetId),
                userId = receiver,
            },
        };
        var err = await Assert.ThrowsAsync<LogicException>(async () =>
        {
            await ts.SendTrade(sender, offers, true);
        });
        Assert.Equal((int)TradesService.SendTradeErrorCodes.BadUserAssets, err.errorCode);
    }
    
    [Fact]
    public async Task Create_One_Trade_Fail_RecipientDoesNotOwnItems_AfterSending()
    {
        var sender = await CreateRandomUser();
        var receiver = await CreateRandomUser();
        var thirdGuy = await CreateRandomUser();
        // make users able to trade
        await SetCanTrade(sender);
        await SetCanTrade(receiver);
                
        var senderItems = await CreateItemsForTrade(sender, 2);
        var receiverItems = await CreateItemsForTrade(receiver, 4);

        using var ts = ServiceProvider.GetOrCreate<TradesService>();
        // create le trade
        var offers = new List<CreateTradeOffer>()
        {
            new()
            {
                robux = null,
                userAssetIds = senderItems.Select(c => c.userAssetId),
                userId = sender,
            },
            new()
            {
                robux = null,
                userAssetIds = receiverItems.Select(c => c.userAssetId),
                userId = receiver,
            },
        };
        await ts.SendTrade(sender, offers, true);
        // Move a sender item to third guy
        await Database.connection.ExecuteAsync("UPDATE user_asset SET user_id = :user_id WHERE id = :id", new
        {
            user_id = thirdGuy,
            id = receiverItems[0].userAssetId,
        });
        // get id
        var inbound = await ts.GetTradesOfType(receiver, TradeType.Inbound, 1, 0);
        var trades = inbound as TradeEntryDb[] ?? inbound.ToArray();
        Assert.Single(trades);
        Assert.True(trades[0].id != 0);
        var tradeId = trades[0].id;
        var err = await Assert.ThrowsAsync<LogicException>(async () =>
        {
            await ts.AcceptTrade(tradeId, receiver);
        });
        Assert.Equal((int)TradesService.SendTradeErrorCodes.BadUserAssets, err.errorCode);
    }
    
    [Fact]
    public async Task Create_One_Trade_Fail_SenderInvalidUserAsset_BeforeSending()
    {
        // note: there is no "AfterSending" variant of this test because we don't need it: the userAsset would be stripped
        // from the assets being transferred in the trade anyway, although we may wan to look into changing that
        // behaviour in the future (e.g. auto declining since customer support deleting a user asset may make the trade
        // unbalanced).
        var sender = await CreateRandomUser();
        var receiver = await CreateRandomUser();
        // make users able to trade
        await SetCanTrade(sender);
        await SetCanTrade(receiver);
                
        var senderItems = await CreateItemsForTrade(sender, 2);
        var receiverItems = await CreateItemsForTrade(receiver, 4);
        // add non-existent item
        senderItems.Add(new TradeItem()
        {
            userAssetId = senderItems[senderItems.Count-1].userAssetId + 100,
            assetId = senderItems[0].assetId,
        });
        using var ts = ServiceProvider.GetOrCreate<TradesService>();
        // create le trade
        var offers = new List<CreateTradeOffer>()
        {
            new()
            {
                robux = null,
                userAssetIds = senderItems.Select(c => c.userAssetId),
                userId = sender,
            },
            new()
            {
                robux = null,
                userAssetIds = receiverItems.Select(c => c.userAssetId),
                userId = receiver,
            },
        };
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await ts.SendTrade(sender, offers, true);
        });
    }
}