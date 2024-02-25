using System.Threading.Tasks;
using Roblox.Models.Economy;
using Roblox.Services;
using Roblox.Services.Exceptions;
using Roblox.Services.IntegrationTest;
using Xunit;

public class CurrencyExchangeTest : TestBase
{
    public async Task MarketOrderWithMultipleSellers()
    {
        var buyer = await CreateRandomUser();
        var sellerOne = await CreateRandomUser();
        using var cx = ServiceProvider.GetOrCreate<CurrencyExchangeService>();
        using var us = ServiceProvider.GetOrCreate<UsersService>();
        using var ec = ServiceProvider.GetOrCreate<EconomyService>();
        // give users money
        await ec.IncrementCurrency(sellerOne, CurrencyType.Tickets, 1000);
        var sellerNewBal = await ec.GetUserBalance(sellerOne);
        Assert.Equal(100, sellerNewBal.robux);
        var buyerNewBal = await ec.GetUserBalance(buyer);
        Assert.Equal(100, buyerNewBal.robux);
        
        // Seller will list 100 robux for 1000 tix, rate is 10
        await cx.PlaceOrder(sellerOne, 1000, 100, CurrencyType.Tickets, false);

        // Make sure balance was subtracted
        sellerNewBal = await ec.GetUserBalance(sellerOne);
        Assert.Equal(0, sellerNewBal.tickets);
        // buyer wants to buy 100 robux at market rate
        await cx.PlaceOrder(buyer, 100, 100, CurrencyType.Robux, true);
        // confirm users have their balance now
        buyerNewBal = await ec.GetUserBalance(buyer);
        Assert.Equal(1000, buyerNewBal.tickets);
        Assert.Equal(0, buyerNewBal.robux);
        // confirm seller has new bal
        sellerNewBal = await ec.GetUserBalance(sellerOne);
        Assert.Equal(200, sellerNewBal.robux); // 100 sold, 100 came with acc
        Assert.Equal(0, sellerNewBal.tickets);
        // confirm position was closed
        var pos = await cx.GetPositionsByUser(sellerOne, CurrencyType.Tickets, 0);
        Assert.Empty(pos);
    }

    public async Task CreateTicketBuyOrder()
    {
        var buyer = await CreateRandomUser();
        var seller = await CreateRandomUser();
        using var cx = ServiceProvider.GetOrCreate<CurrencyExchangeService>();
        using var us = ServiceProvider.GetOrCreate<UsersService>();
        using var ec = ServiceProvider.GetOrCreate<EconomyService>();
        // give users money
        await ec.IncrementCurrency(seller, CurrencyType.Tickets, 1000);
        var sellerNewBal = await ec.GetUserBalance(seller);
        Assert.Equal(100, sellerNewBal.robux);
        var buyerNewBal = await ec.GetUserBalance(buyer);
        Assert.Equal(100, buyerNewBal.robux);
        
        // Seller will list 100 robux for 1000 tix, rate is 10
        await cx.PlaceOrder(seller, 1000, 100, CurrencyType.Tickets, false);

        // Make sure balance was subtracted
        sellerNewBal = await ec.GetUserBalance(seller);
        Assert.Equal(0, sellerNewBal.tickets);
        // buyer wants to buy 100 robux for 1000 tix
        await cx.PlaceOrder(buyer, 100, 10 * 1000, CurrencyType.Robux, false);
        // confirm users have their balance now
        buyerNewBal = await ec.GetUserBalance(buyer);
        Assert.Equal(1000, buyerNewBal.tickets);
        Assert.Equal(0, buyerNewBal.robux);
        // confirm seller has new bal
        sellerNewBal = await ec.GetUserBalance(seller);
        Assert.Equal(200, sellerNewBal.robux); // 100 sold, 100 came with acc
        Assert.Equal(0, sellerNewBal.tickets);
        // confirm position was closed
        var pos = await cx.GetPositionsByUser(seller, CurrencyType.Tickets, 0);
        Assert.Empty(pos);
    }
    
    public async Task CreateRobuxBuyOrder()
    {
        var buyer = await CreateRandomUser();
        var seller = await CreateRandomUser();
        using var cx = ServiceProvider.GetOrCreate<CurrencyExchangeService>();
        using var us = ServiceProvider.GetOrCreate<UsersService>();
        using var ec = ServiceProvider.GetOrCreate<EconomyService>();
        // give users money
        await ec.IncrementCurrency(buyer, CurrencyType.Tickets, 1000);
        // await us.IncrementCurrency(seller, CurrencyType.Robux, 100); // you start with a bal of 100
        var sellerNewBal = await ec.GetUserBalance(seller);
        Assert.Equal(100, sellerNewBal.robux);
        var buyerNewBal = await ec.GetUserBalance(buyer);
        Assert.Equal(100, buyerNewBal.robux);
        
        // Seller will list 100 robux for 1000 tix, rate is 10
        await cx.PlaceOrder(seller, 100, 10 * 1000, CurrencyType.Robux, false);
        // Make sure balance was subtracted
        sellerNewBal = await ec.GetUserBalance(seller);
        Assert.Equal(0, sellerNewBal.robux);
        // buyer wants to buy 100 robux for 1000 tix
        await cx.PlaceOrder(buyer, 1000, 100, CurrencyType.Tickets, false);
        // confirm users have their balance now
        buyerNewBal = await ec.GetUserBalance(buyer);
        Assert.Equal(0, buyerNewBal.tickets);
        Assert.Equal(200, buyerNewBal.robux); // 100 purchased, 100 came with the account
        // confirm seller has new bal
        sellerNewBal = await ec.GetUserBalance(seller);
        Assert.Equal(0, sellerNewBal.robux);
        Assert.Equal(1000, sellerNewBal.tickets);
        // confirm position was closed
        var pos = await cx.GetPositionsByUser(seller, CurrencyType.Robux, 0);
        Assert.Empty(pos);
    }
    
    public async Task CreateRobuxBuyOrder_BadAmounts()
    {
        var seller = await CreateRandomUser();
        using var cx = ServiceProvider.GetOrCreate<CurrencyExchangeService>();
        using var us = ServiceProvider.GetOrCreate<UsersService>();
        using var ec = ServiceProvider.GetOrCreate<EconomyService>();

        // await us.IncrementCurrency(seller, CurrencyType.Robux, 100); // you start with a bal of 100
        var sellerNewBal = await ec.GetUserBalance(seller);
        Assert.Equal(100, sellerNewBal.robux);

        // Try negative amount
        await Assert.ThrowsAsync<RobloxException>(async () =>
        {
            await cx.PlaceOrder(seller, -100, 10 * 1000, CurrencyType.Robux, false);
        });
        // Try positive, but over the current balance
        await Assert.ThrowsAsync<RobloxException>(async () =>
        {
            await cx.PlaceOrder(seller, sellerNewBal.robux + 1, 10 * 1000, CurrencyType.Robux, false);
        });
        // Try bad exchange rate (would yield negative return)
        await Assert.ThrowsAsync<RobloxException>(async () =>
        {
            await cx.PlaceOrder(seller, 1, 10000, CurrencyType.Robux, false);
        });
        // confirm balance never changed
        sellerNewBal = await ec.GetUserBalance(seller);
        Assert.Equal(100, sellerNewBal.robux);
    }
}