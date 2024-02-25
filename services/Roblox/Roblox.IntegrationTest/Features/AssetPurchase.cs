using System.Linq;
using System.Threading.Tasks;
using Roblox.Dto.Users;
using Roblox.Exceptions;
using Roblox.Models.Economy;
using Roblox.Services;
using Roblox.Services.IntegrationTest;
using Roblox.Website.Controllers;
using Xunit;

public class AssetPurchaseTest : TestBase
{
    [Fact]
    public async Task Create_Item_And_Buy_It()
    {
        var buyer = await CreateRandomUser();
        var creator = await CreateRandomUser();
        var item = await CreateRandomItem(creator);
        var assets = new AssetsService();
        await assets.UpdateAssetMarketInfo(item, true, false, false,
            null, null);
        await assets.SetItemPrice(item, 10, null);
        var economyController = ControllerAuth<EconomyControllerV1>(buyer);
        await economyController.PurchaseAsset(item, new PurchaseRequest()
        {
            expectedPrice = 10,
            expectedSellerId = creator,
            expectedCurrency = CurrencyType.Robux,
        });
        // confirm balances were updated
        var economy = new EconomyService();
        var balOne = await economy.GetUserRobux(buyer);
        Assert.Equal(90, balOne);
        var balTwo = await economy.GetUserRobux(creator);
        Assert.Equal(107, balTwo);
        // confirm transaction exists
        var usersService = new UsersService();
        var trans = await economy.GetTransactions(buyer, PurchaseType.Purchase, 1, 0);
        var realTrans = trans.ToList();
        Assert.Single(realTrans);
        Assert.Equal(10, realTrans[0].amount);
        Assert.Equal(PurchaseType.Purchase, realTrans[0].type);
        Assert.Equal(item, realTrans[0].assetId);
        Assert.Equal(creator, realTrans[0].userIdTwo);
        Assert.Null(realTrans[0].groupIdTwo);
        // confirm trans exists for seller too
        var sellerTrans = (await economy.GetTransactions(creator, PurchaseType.Sale, 1, 0)).ToList();
        Assert.Single(sellerTrans);
        Assert.Equal(7, sellerTrans[0].amount);
        Assert.Equal(buyer, sellerTrans[0].userIdTwo);
        Assert.Equal(item, sellerTrans[0].assetId);
        Assert.Null(realTrans[0].groupIdTwo);
        // finally, confirm user actually got the item
        var itemResult = await usersService.GetUserAssets(buyer, item);
        var items = itemResult.ToList();
        Assert.Single(items);
        Assert.Equal(item, items[0].assetId);
        Assert.Equal(0, items[0].price);
        Assert.Equal(buyer, items[0].userId);
    }
    
    [Fact]
    public async Task Create_LimitedUnique_Item_And_Buy_It()
    {
        var buyerOne = await CreateRandomUser();
        var buyerTwo = await CreateRandomUser();
        var buyerThree = await CreateRandomUser();
        var creator = await CreateRandomUser();
        var item = await CreateRandomItem(creator);
        var economyService = new EconomyService();
        var creatorBalAtStart = await economyService.GetUserRobux(creator);
        Assert.Equal(100, creatorBalAtStart);
        var assets = new AssetsService();
        // Max copies of 2
        await assets.UpdateAssetMarketInfo(item, true, true, true, 
            2, null);
        await assets.SetItemPrice(item, 10, null);
        var economyController = ControllerAuth<EconomyControllerV1>(buyerOne);
        // Buyer one purchase, gets serial 1
        await economyController.PurchaseAsset(item, new PurchaseRequest()
        {
            expectedPrice = 10,
            expectedSellerId = creator,
            expectedCurrency = CurrencyType.Robux,
        });
        // Below is all for first purchase
        // confirm balances were updated
        var usersService = new UsersService();
        var balOne = await economyService.GetUserRobux(buyerOne);
        Assert.Equal(90, balOne);
        var balTwo = await economyService.GetUserRobux(creator);
        Assert.Equal(107, balTwo);
        // confirm transaction exists
        var trans = await economyService.GetTransactions(buyerOne, PurchaseType.Purchase, 1, 0);
        var realTrans = trans.ToList();
        Assert.Single(realTrans);
        Assert.Equal(10, realTrans[0].amount);
        Assert.Equal(PurchaseType.Purchase, realTrans[0].type);
        Assert.Equal(item, realTrans[0].assetId);
        Assert.Equal(creator, realTrans[0].userIdTwo);
        // confirm trans exists for seller too
        var sellerTrans = (await economyService.GetTransactions(creator, PurchaseType.Sale, 1, 0)).ToList();
        Assert.Single(sellerTrans);
        Assert.Equal(7, sellerTrans[0].amount);
        Assert.Equal(buyerOne, sellerTrans[0].userIdTwo);
        Assert.Equal(item, sellerTrans[0].assetId);
        // finally, confirm user actually got the item
        var itemResult = await usersService.GetUserAssets(buyerOne, item);
        var items = itemResult.ToList();
        Assert.Single(items);
        Assert.Equal(item, items[0].assetId);
        Assert.Equal(0, items[0].price);
        Assert.Equal(buyerOne, items[0].userId);
        Assert.Equal(1, items[0].serial);
        
        // buyer one attempt to purchase again, should fail since already owns
        await Assert.ThrowsAsync<BadRequestException>(async () =>
        {
            await economyController.PurchaseAsset(item, new() {expectedPrice = 10, expectedSellerId = creator, expectedCurrency = CurrencyType.Robux});
        });
        
        // Buyer two purchase, gets serial 2
        var buyerTwoCtrl = ControllerAuth<EconomyControllerV1>(buyerTwo);
        await buyerTwoCtrl.PurchaseAsset(item, new() {expectedPrice = 10, expectedSellerId = creator, expectedCurrency = CurrencyType.Robux});
        // Buyer three purchase, fail because item should no longer be for sale
        var result = await Assert.ThrowsAsync<BadRequestException>(async () =>
        {
            var buyerThreeCtrl = ControllerAuth<EconomyControllerV1>(buyerThree);
            await buyerThreeCtrl.PurchaseAsset(item, new() {expectedPrice = 10, expectedSellerId = creator, expectedCurrency = CurrencyType.Robux});
        });
        // balance of three should not be updated
        var buyerThreeBal = await economyService.GetUserRobux(buyerThree);
        Assert.Equal(100, buyerThreeBal);
    }
    
    [Fact]
    public async Task Create_LimitedUnique_Item_And_Buy_It_Then_Resell_To_Another_User()
    {
        var buyerOne = await CreateRandomUser();
        var buyerTwo = await CreateRandomUser();
        var buyerThree = await CreateRandomUser();
        var creator = await CreateRandomUser();
        var item = await CreateRandomItem(creator);
        var assets = new AssetsService();
        using var us = ServiceProvider.GetOrCreate<UsersService>();
        // Max copies of 1
        await assets.UpdateAssetMarketInfo(item, true, true, true, 
            1, null);
        await assets.SetItemPrice(item, 10, null);
        var economyController = ControllerAuth<EconomyControllerV1>(buyerOne);
        // Buyer one purchase, gets serial 1
        await economyController.PurchaseAsset(item, new PurchaseRequest()
        {
            expectedPrice = 10,
            expectedSellerId = creator,
            expectedCurrency = CurrencyType.Robux,
        });
        var copies = (await us.GetUserAssets(buyerOne, item)).FirstOrDefault();
        Assert.NotNull(copies);
        // Item can now be re-sold.
        // Set the price
        await economyController.SetPriceOfUserAsset(item, copies.userAssetId, new SetPriceRequest()
        {
            price = 100,
        });
        long amountSellerShouldGet = 70;
        // confirm price was set
        copies = (await us.GetUserAssets(buyerOne, item)).First();
        Assert.Equal(100, copies.price);
        Assert.Equal(copies.createdAt, copies.updatedAt);
        
        var economyTwo = ControllerAuth<EconomyControllerV1>(buyerTwo);
        // now try to buy with user two
        await economyTwo.PurchaseAsset(item, new PurchaseRequest()
        {
            expectedCurrency = CurrencyType.Robux,
            expectedPrice = copies.price,
            expectedSellerId = buyerOne,
            userAssetId = copies.userAssetId,
        });
        // confirm user got it
        var buyerTwoCopies = (await us.GetUserAssets(buyerTwo, item)).ToArray();
        Assert.Single(buyerTwoCopies);
        // price should always be reset
        Assert.Equal(0, buyerTwoCopies[0].price);
        // updated at should be set after purchase
        Assert.True(buyerTwoCopies[0].updatedAt > buyerTwoCopies[0].createdAt);
        
        // Below is second purchase
        // confirm balances were updated
        // Price was 70, seller bought at 10 robux which brought down to 90, then sold for 100 which brought up to 160 
        var economyService = new EconomyService();
        var balOne = await economyService.GetUserRobux(buyerOne);
        Assert.Equal(160, balOne);
        var balTwo = await economyService.GetUserRobux(buyerTwo);
        Assert.Equal(0, balTwo); // starts with 100. should be 0 because bought item for 100
        // confirm transaction exists
        var trans = await economyService.GetTransactions(buyerTwo, PurchaseType.Purchase, 1, 0);
        var realTrans = trans.ToList();
        Assert.Single(realTrans);
        Assert.Equal(100, realTrans[0].amount);
        Assert.Equal(PurchaseType.Purchase, realTrans[0].type);
        Assert.Equal(TransactionSubType.ItemResalePurchase, realTrans[0].subType);
        Assert.Equal(item, realTrans[0].assetId);
        Assert.Equal(buyerOne, realTrans[0].userIdTwo);
        
        // confirm trans exists for seller too
        var sellerTrans = (await economyService.GetTransactions(buyerOne, PurchaseType.Sale, 1, 0)).ToList();
        Assert.Single(sellerTrans);
        Assert.Equal(amountSellerShouldGet, sellerTrans[0].amount);
        Assert.Equal(buyerTwo, sellerTrans[0].userIdTwo);
        Assert.Equal(item, sellerTrans[0].assetId);
        Assert.Equal(TransactionSubType.ItemResale, sellerTrans[0].subType);
    }

    private async Task AfterPurchaseFailure(long buyerId, long sellerId)
    {
        // confirm balances were NOT updated
        var usersService = new UsersService();
        var economyService = new EconomyService();
        var balOne = await economyService.GetUserRobux(buyerId);
        Assert.Equal(100, balOne);
        var balTwo = await economyService.GetUserRobux(sellerId);
        Assert.Equal(100, balTwo);
        // confirm transactions were not created
        var buyerTrans = (await economyService.GetTransactions(buyerId, PurchaseType.Purchase, 100, 0));
        Assert.Empty(buyerTrans);
        var sellerTrans = await economyService.GetTransactions(sellerId, PurchaseType.Sale, 100, 0);
        Assert.Empty(sellerTrans);
    }
    
    [Fact]
    public async Task AttemptPurchase_Fail_NotEnoughRobux()
    {
        var buyer = await CreateRandomUser();
        var creator = await CreateRandomUser();
        var item = await CreateRandomItem(creator);
        var assets = new AssetsService();
        await assets.UpdateAssetMarketInfo(item, true, false, false, 1000, 
            null, null);
        var economyController = ControllerAuth<EconomyControllerV1>(buyer);
        await Assert.ThrowsAsync<BadRequestException>(async () =>
        {
            await economyController.PurchaseAsset(item, new PurchaseRequest()
            {
                expectedPrice = 1000,
                expectedSellerId = creator,
                expectedCurrency = CurrencyType.Robux,
            });
        });
        await AfterPurchaseFailure(buyer, creator);
    }
    
    [Fact]
    public async Task AttemptPurchase_Fail_ItemNotForSale()
    {
        var buyer = await CreateRandomUser();
        var creator = await CreateRandomUser();
        var item = await CreateRandomItem(creator);
        var assets = new AssetsService();
        await assets.UpdateAssetMarketInfo(item, false, false, false, 1000, 
            null, null);
        var economyController = ControllerAuth<EconomyControllerV1>(buyer);
        // price is correct but item is not for sale
        await Assert.ThrowsAsync<BadRequestException>(async () =>
        {
            await economyController.PurchaseAsset(item, new PurchaseRequest()
            {
                expectedPrice = 1000,
                expectedSellerId = creator,
                expectedCurrency = CurrencyType.Robux,
            });
        });
        await AfterPurchaseFailure(buyer, creator);
    }
    
    [Fact]
    public async Task AttemptPurchase_Fail_WrongExpectedPrice()
    {
        var buyer = await CreateRandomUser();
        var creator = await CreateRandomUser();
        var item = await CreateRandomItem(creator);
        var assets = new AssetsService();
        await assets.UpdateAssetMarketInfo(item, true, false, false, 100, 
            null, null);
        var economyController = ControllerAuth<EconomyControllerV1>(buyer);
        // price is incorrect
        await Assert.ThrowsAsync<BadRequestException>(async () =>
        {
            await economyController.PurchaseAsset(item, new PurchaseRequest()
            {
                expectedPrice = 1000,
                expectedSellerId = creator,
                expectedCurrency = CurrencyType.Robux,
            });
        });
        await AfterPurchaseFailure(buyer, creator);
    }
}