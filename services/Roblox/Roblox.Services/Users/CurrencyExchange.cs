using System.Diagnostics;
using Dapper;
using Roblox.Dto.Users;
using Roblox.Logging;
using Roblox.Models.Economy;
using Roblox.Services.Exceptions;

namespace Roblox.Services;

public class CurrencyExchangeService : ServiceBase, IService
{
    public async Task<IEnumerable<Dto.Users.TradeCurrencyOrder>> GetOrdersMatchingCriteria(CurrencyType sourceCurrency,
        long maxExchangeRate1K)
    {
        var query = await db.QueryAsync<Dto.Users.TradeCurrencyOrder>(
            "SELECT * FROM trade_currency_order WHERE exchange_rate <= :max_exchange_rate AND destination_currency = :currency AND NOT is_closed AND balance > 0 ORDER BY exchange_rate ASC",
            new
            {
                currency = sourceCurrency,
                max_exchange_rate = maxExchangeRate1K,
            });
        return query;
    }

    public async Task<IEnumerable<Dto.Users.CurrencyExchangeMarketTotalEntry>> GetPositionsGroupByRate(
        CurrencyType sourceCurrency)
    {
        var query = await db.QueryAsync<Dto.Users.TradeCurrencyOrder>(
            "SELECT exchange_rate, balance FROM trade_currency_order WHERE source_currency = :currency AND NOT is_closed AND balance > 0 ORDER BY exchange_rate ASC LIMIT 1000",
            new
            {
                currency = sourceCurrency,
            });

        var result = new List<CurrencyExchangeMarketTotalEntry>();
        foreach (var item in query)
        {
            var exists = result.Find(v => v.rate == item.exchangeRate);
            if (exists != null)
            {
                exists.amount += item.balance;
            }
            else
            {
                if (result.Count < 20)
                    result.Add(new CurrencyExchangeMarketTotalEntry()
                    {
                        rate = item.exchangeRate,
                        amount = item.balance,
                    });
            }
        }

        return result;
    }

    public async Task<long> CountPositionsByUser(long userId)
    {
        var total = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) AS total FROM trade_currency_order WHERE user_id = :user_id AND NOT is_closed AND balance > 0",
            new
            {
                user_id = userId,
            });
        return total.total;
    }

    public async Task<IEnumerable<Dto.Users.TradeCurrencyOrder>> GetPositionsByUser(long userId,
        CurrencyType sourceCurrency, long startId)
    {
        var qu = new SqlBuilder();
        var t = qu.AddTemplate("SELECT * FROM trade_currency_order /**where**/ /**orderby**/");
        qu.OrderBy("id DESC");
        qu.Where("user_id = :user_id", new
        {
            user_id = userId,
        });
        qu.Where("source_currency = :c", new
        {
            c = sourceCurrency,
        });
        qu.Where("balance > 0 AND NOT is_closed");
        if (startId != 0)
        {
            qu.Where("id > :id", new
            {
                id = startId,
            });
        }

        var query = await db.QueryAsync<Dto.Users.TradeCurrencyOrder>(t.RawSql, t.Parameters);
        return query;
    }

    public async Task<IEnumerable<Dto.Users.TradeCurrencyOrder>> GetAllOrders(CurrencyType sourceCurrency)
    {
        var query = await db.QueryAsync<Dto.Users.TradeCurrencyOrder>(
            "SELECT * FROM trade_currency_order WHERE source_currency = :currency AND NOT is_closed AND balance > 0 ORDER BY exchange_rate ASC",
            new
            {
                currency = sourceCurrency,
            });
        return query;
    }

    public async Task<Dto.Users.TradeCurrencyOrder?> GetOrderById(long orderId)
    {
        return await db.QuerySingleOrDefaultAsync<TradeCurrencyOrder?>(
            "SELECT * FROM trade_currency_order WHERE id = :id", new
            {
                id = orderId,
            });
    }

    private async Task InsertExchangeLog(long orderId, long userId, long sourceAmount, long destinationAmount)
    {
        await InsertAsync("trade_currency_log", new
        {
            order_id = orderId,
            user_id = userId,
            source_amount = sourceAmount,
            destination_amount = destinationAmount,
        });
    }

    private async Task<bool> AttemptBuyOrder(long sourceOrderId, long toBuyOrderId, long desiredAmount,
        long amountToTakeFromSource)
    {
        var logger = Writer.CreateWithId(LogGroup.CurrencyExchange);
        logger.Info($"attempt buy order {toBuyOrderId} for {sourceOrderId}. amt = {desiredAmount}");
#if RELEASE
        await using var positionLock = await GetLockForOrder(toBuyOrderId);
#endif
        var sourceData = await GetOrderById(sourceOrderId);
        var destData = await GetOrderById(toBuyOrderId);
        // source is buying dest
        if (destData == null || destData.balance < desiredAmount || sourceData == null || sourceOrderId == toBuyOrderId)
        {
            logger.Info($"failed {destData == null} bal={destData?.balance}");
            return false;
        }
        
        logger.Info("amountToTakeFromSource: {0}", amountToTakeFromSource);
        logger.Info("source balance = {0}", sourceData.balance);
        if (amountToTakeFromSource > sourceData.balance)
            return false;
        if (amountToTakeFromSource < 1)
            throw new ArgumentException(nameof(amountToTakeFromSource) + " cannot be less than 1: " + amountToTakeFromSource);
        if (desiredAmount < 1)
            throw new ArgumentException(nameof(desiredAmount) + " cannot be less than 1: " + desiredAmount);
        if (sourceData.balance < 1)
            throw new Exception("Source balance was less than 1: " + sourceData.balance);
        // We can complete the order now!
        await DecrementFromBalance(sourceOrderId, amountToTakeFromSource);
        await DecrementFromBalance(toBuyOrderId, desiredAmount);
        var ec = ServiceProvider.GetOrCreate<EconomyService>(this);
        // Transfer
        // Give source user the money they purchased
        await ec.IncrementCurrency(sourceData.userId, destData.sourceCurrency, desiredAmount);
        // Give dest user the money they earned
        await ec.IncrementCurrency(destData.userId, destData.destinationCurrency, amountToTakeFromSource);
        // Exchange transactions
        await InsertExchangeLog(sourceOrderId, destData.userId, desiredAmount, amountToTakeFromSource);
        await InsertExchangeLog(toBuyOrderId, sourceData.userId, amountToTakeFromSource, desiredAmount);
        // User transactions
        // Transaction for seller:
        await InsertAsync("user_transaction", new
        {
            type = PurchaseType.Sale,
            currency_type =  destData.destinationCurrency,
            amount = amountToTakeFromSource,
            sub_type = TransactionSubType.PositionSale,
            user_id_one = destData.userId,
            user_id_two = 1,
        });
        // Transaction for buyer:
        await InsertAsync("user_transaction", new
        {
            type = PurchaseType.Sale,
            currency_type =  destData.sourceCurrency,
            amount = desiredAmount,
            sub_type = TransactionSubType.PositionSale,
            user_id_one = sourceData.userId,
            user_id_two = 1,
        });
        return true;
    }

    private async Task<IAsyncDisposable> GetLockForOrder(long orderId)
    {
        var result = await Cache.redLock.CreateLockAsync("BuyOrder:V1:" + orderId, TimeSpan.FromSeconds(2));
        if (result == null || result.IsAcquired == false)
            throw new LockNotAcquiredException();
        return result;
    }

    private async Task CloseOrder(long orderId)
    {
        await db.ExecuteAsync("UPDATE trade_currency_order SET closed_at = :dt WHERE id = :id", new
        {
            dt = DateTime.UtcNow,
            id = orderId,
        });
    }

    private async Task DecrementFromBalance(long orderId, long amount)
    {
        if (amount < 1)
            throw new ArgumentException("Cannot deduct less than 1 from balance: " + amount);
        
        await db.ExecuteAsync("UPDATE trade_currency_order SET balance = balance - :amt WHERE id = :id", new
        {
            id = orderId,
            amt = amount,
        });
    }

    private async Task AttemptBuyForPosition(long orderId, bool isMarketOrder)
    {
        var logger = Writer.CreateWithId(LogGroup.CurrencyExchange);
        logger.Info("AttemptBuyForPosition {0}", orderId);
        await using var positionLock = await GetLockForOrder(orderId);
        var theOrder = await GetOrderById(orderId);
        if (theOrder == null)
            return; // just ignore for now
        logger.Info($"Destination currency is {theOrder.destinationCurrency}, source is {theOrder.sourceCurrency}");
        var matches = await GetAllOrders(theOrder.destinationCurrency);
        // This is the balance in the source currency. We need to convert it to dest currency.
        var balance = theOrder.balance;
        logger.Info("starting balance is {0}", balance);
        foreach (var match in matches)
        {
            if (match.balance <= 0)
                continue;
            
            if (match.userId == theOrder.userId)
            {
                logger.Info($"skip trade with own user. id={match.id} userId={match.userId}");
                continue;
            }

            logger.Info($"match. id={match.id} rate={match.exchangeRate} balance={match.balance}");
            // This is the remaining balance in dest currency
            var trueRate = (decimal) match.exchangeRate / 1000;
            
            var trueRateReal = 1 / trueRate;
            logger.Info($"real rate is: {trueRate} - real: {trueRateReal}");
            var ourBalanceConverted = (long) Math.Truncate((decimal) balance * trueRateReal);
            

            logger.Info("current balance at the users exchange rate: {0}", ourBalanceConverted);
            if (ourBalanceConverted > match.balance)
            {
                logger.Info($"match does not have enough, skipping. match balance={match.balance}");
                continue;
            }

            var desiredExchangeRate = balance * (theOrder.exchangeRate / 1000);
            if (!isMarketOrder && ourBalanceConverted < desiredExchangeRate)
            {
                logger.Info($"skip because rate is too low. wanted {desiredExchangeRate}, got {ourBalanceConverted}");
                continue;
            }

            var amountToBuy = ourBalanceConverted;
            var amountToTake = balance;
            logger.Info("amount to buy={0}", amountToBuy);
            if (amountToBuy < 1 || amountToTake < 1)
                throw new Exception("Attempted to buy " + amountToBuy +" and take " + amountToTake);
            
            if (await AttemptBuyOrder(orderId, match.id, amountToBuy, amountToTake))
            {
                var newInfo = await GetOrderById(orderId);
                if (newInfo == null)
                    throw new Exception("Order " + orderId + " does not exist");
                balance = newInfo.balance;
                logger.Info("buy success. remaining balance = {0}", balance);
                if (balance < 0)
                    throw new Exception("Balance was less than zero after purchase");
            }

            if (balance == 0)
            {
                // No longer for sale
                logger.Info("done purchasing, balance is zero");
                await CloseOrder(orderId);
                break;
            }
        }
    }

    public async Task CloseOrder(long userId, long orderId)
    {
        await using var loc = await GetLockForOrder(orderId);
        var details = await GetOrderById(orderId);
        if (details == null || details.userId != userId || details.isClosed || details.balance == 0)
            return;

        await db.ExecuteAsync("UPDATE trade_currency_order SET is_closed = true WHERE id = :id", new
        {
            id = orderId,
        });
        if (details.balance != 0)
        {
            using var ec = ServiceProvider.GetOrCreate<EconomyService>(this);
            await ec.IncrementCurrency(details.userId, details.sourceCurrency, details.balance);
        }
    }

    public async Task<long> GetAverageRate(CurrencyType sourceCurrency)
    {
        var result = await db.QuerySingleOrDefaultAsync(
            "SELECT avg(exchange_rate) as avg FROM trade_currency_order WHERE source_currency = :source AND NOT is_closed AND balance > 0",
            new
            {
                source = sourceCurrency,
            });
        return (long) Math.Ceiling((decimal) (result.avg == null ? 0 : result.avg));
    }

    public async Task<long> GetHigh(CurrencyType sourceCurrency)
    {
        var result = await db.QuerySingleOrDefaultAsync(
            "SELECT max(exchange_rate) as high FROM trade_currency_log INNER JOIN trade_currency_order ON trade_currency_order.id = trade_currency_log.order_id WHERE source_currency = :source AND NOT is_closed AND balance > 0 AND trade_currency_log.created_at >= :d",
            new
            {
                source = sourceCurrency,
                d = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)),
            });
        return (long) Math.Ceiling((decimal) (result.high == null ? 0 : result.high));
    }

    public async Task<long> GetLow(CurrencyType sourceCurrency)
    {
        var result = await db.QuerySingleOrDefaultAsync(
            "SELECT max(exchange_rate) as low FROM trade_currency_log INNER JOIN trade_currency_order ON trade_currency_order.id = trade_currency_log.order_id WHERE source_currency = :source AND NOT is_closed AND balance > 0 AND trade_currency_log.created_at >= :d",
            new
            {
                source = sourceCurrency,
                d = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)),
            });
        return (long) Math.Ceiling((decimal) (result.low == null ? 0 : result.low));
    }

    private async Task<bool> IsPurchaseFloodChecked(long userId, long amount, CurrencyType sourceCurrency)
    {
        var dest = sourceCurrency == CurrencyType.Robux ? CurrencyType.Tickets : CurrencyType.Robux;
        var result = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT count(*) as total FROM trade_currency_log l INNER JOIN trade_currency_order tco on l.order_id = tco.id WHERE l.user_id = :user_id AND l.created_at >= :d AND tco.source_currency = :source AND tco.destination_currency = :dest",
            new
            {
                user_id = userId,
                source = sourceCurrency,
                dest = dest,
                d = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5)),
            });
        // TODO: influx?
        return result != null && result.total > 10;
    }

    public async Task PlaceOrder(long userId, long amount, long exchangeRate1K, CurrencyType sourceCurrency, bool isMarketOrder)
    {
        if (exchangeRate1K <= 0 || exchangeRate1K >= (1000 * 10000))
            throw new RobloxException(400, 0, "BadRequest");

        if ((amount * 1000) % exchangeRate1K != 0)
            throw new RobloxException(400, 0, "Bad exchange rate");

        // Confirm raw amount is above zero
        if (amount < 1)
            throw new RobloxException(400, 0, "Bad amount");
        
        // Confirm converted amount would be above zero
        // This is the remaining balance in dest currency
        var trueRate = (decimal)exchangeRate1K/ 1000;
        var trueRateReal = 1 / trueRate;
        var ourBalanceConverted = (long) Math.Truncate((decimal) amount * trueRateReal);
        if (ourBalanceConverted < 1)
            throw new RobloxException(400, 0, "Bad exchange rate");
        
        // WEB-68: this is probably temporary
        // flood check
        if (await IsPurchaseFloodChecked(userId, amount, sourceCurrency))
        {
            throw new RobloxException(400, 0, "Too many attempts. Try again in a few minutes.");
        }

        var logger = Writer.CreateWithId(LogGroup.CurrencyExchange);
        logger.Info(
            $"new order created. userId={userId} amount={amount} rate={exchangeRate1K} sourceCurrency={sourceCurrency} isMarket={isMarketOrder}");
        logger.Info($"attempt decrement from user balance: {amount} {sourceCurrency.ToString()}");
        await InTransaction(async (trx) =>
        {
            // Confirm user has enough
            using var ec = ServiceProvider.GetOrCreate<EconomyService>(this);
            var currency = await ec.GetUserBalance(userId);
            var balance = sourceCurrency == CurrencyType.Robux ? currency.robux : currency.tickets;
            if (balance < amount)
            {
                throw new RobloxException(400, 0, "You do not have enough currency to make this trade");
            }
            // decrement
            await ec.DecrementCurrency(userId, sourceCurrency, amount);
            // Record the position first
            var orderId = await InsertAsync("trade_currency_order", "id", new
            {
                user_id = userId,
                start_amount = amount,
                balance = amount,
                exchange_rate = exchangeRate1K,
                source_currency = sourceCurrency,
                destination_currency = sourceCurrency == CurrencyType.Robux ? CurrencyType.Tickets : CurrencyType.Robux,
                is_closed = false,
            });
            // try to buy position
            await AttemptBuyForPosition(orderId, isMarketOrder);
            return 0;
        });
    }

    public bool IsThreadSafe()
    {
        return false;
    }

    public bool IsReusable()
    {
        return false;
    }
}