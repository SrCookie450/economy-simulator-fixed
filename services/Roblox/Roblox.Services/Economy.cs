using Dapper;
using Roblox.Dto.Economy;
using Roblox.Dto.Users;
using Roblox.Libraries.Exceptions;
using Roblox.Models.Assets;
using Roblox.Models.Economy;
using Roblox.Services.Exceptions;

namespace Roblox.Services;

public class EconomyService : ServiceBase, IService
{
    public async Task<long> GetUserRobux(long userId)
    {
        return (await GetUserBalance(userId)).robux;
    }

    public async Task<UserEconomy> GetBalance(CreatorType type, long creatorId)
    {
        if (type == CreatorType.User)
            return await GetUserBalance(creatorId);
        if (type == CreatorType.Group)
            return await GetGroupBalance(creatorId);
        throw new Exception("Invalid creatorType: " + type);
    }

    public async Task<UserEconomy> GetUserBalance(long userId)
    {
        var result = await db.QuerySingleOrDefaultAsync<UserEconomy?>(
            "SELECT balance_robux As robux, balance_tickets as tickets FROM user_economy WHERE user_id = :user_id", new { user_id = userId });
        // Some early users have no economy
        if (result == null)
            throw new Exception("User does not have an economy entry");
        // NOTE: We could do this, but it might break things that check the balance AFTER subtracting/adding
        /*
        {
            await db.ExecuteAsync(
                "INSERT INTO user_economy (user_id, balance_robux, balance_tickets) VALUES (:user_id, 0, 0)", new
                {
                    user_id = userId,
                });
        }
        */
        return result;
    }
    
    public async Task<IAsyncDisposable> AcquireEconomyLock(CreatorType creatorType, long creatorId)
    {
        if (!Enum.IsDefined(creatorType))
            throw new ArgumentException("Invalid creatorType");
        var result = await Cache.redLock.CreateLockAsync("EconomyLockV2:"+creatorType.ToString()+":" + creatorId, TimeSpan.FromSeconds(5));
        if (!result.IsAcquired)
            throw new LockNotAcquiredException();
        return result;
    }

    public async Task CreateGroupBalanceIfRequired(long groupId)
    {
        var exists = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) as total FROM group_economy WHERE group_id = :group_id", new
            {
                group_id = groupId,
            });
        if (exists.total == 0)
        {
            await db.ExecuteAsync(
                "INSERT INTO group_economy (group_id, balance_robux, balance_tickets) VALUES (:group_id, 0, 0)", new
                {
                    group_id = groupId,
                });
        }
    }
    
    private async Task<UserEconomy> GetGroupBalance(long groupId)
    {
        var result = await db.QuerySingleOrDefaultAsync<UserEconomy?>(
            "SELECT balance_robux As robux, balance_tickets as tickets FROM group_economy WHERE group_id = :group_id", new { group_id = groupId });
        // Some early users have no economy
        if (result == null)
            throw new Exception("User does not have an economy entry");
        return result;
    }

    private async Task UnsafeIncrementUserRobux(long userId, long amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount must be zero or more");
        await db.ExecuteAsync("UPDATE user_economy SET balance_robux = balance_robux + :amt WHERE user_id = :user_id",
            new
            {
                user_id = userId,
                amt = amount,
            });
    }
    
    private async Task UnsafeIncrementUserTickets(long userId, long amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount must be zero or more");
        await db.ExecuteAsync("UPDATE user_economy SET balance_tickets = balance_tickets + :amt WHERE user_id = :user_id",
            new
            {
                user_id = userId,
                amt = amount,
            });
    }
    
    private async Task UnsafeDecrementUserRobux(long userId, long amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount must be zero or more");
        await db.ExecuteAsync("UPDATE user_economy SET balance_robux = balance_robux - :amt WHERE user_id = :user_id",
            new
            {
                user_id = userId,
                amt = amount,
            });
    }
    
    private async Task UnsafeDecrementUserTickets(long userId, long amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount must be zero or more");
        await db.ExecuteAsync("UPDATE user_economy SET balance_tickets = balance_tickets - :amt WHERE user_id = :user_id",
            new
            {
                user_id = userId,
                amt = amount,
            });
    }
    
    private async Task UnsafeIncrementGroupRobux(long groupId, long amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount must be zero or more");
        await db.ExecuteAsync("UPDATE group_economy SET balance_robux = balance_robux + :amt WHERE group_id = :group_id",
            new
            {
                group_id = groupId,
                amt = amount,
            });
    }
    
    private async Task UnsafeIncrementGroupTickets(long groupId, long amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount must be zero or more");
        await db.ExecuteAsync("UPDATE group_economy SET balance_tickets = balance_tickets + :amt WHERE group_id = :group_id",
            new
            {
                group_id = groupId,
                amt = amount,
            });
    }
    
    private async Task UnsafeDecrementGroupRobux(long groupId, long amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount must be zero or more");
        await db.ExecuteAsync("UPDATE group_economy SET balance_robux = balance_robux - :amt WHERE group_id = :group_id",
            new
            {
                group_id = groupId,
                amt = amount,
            });
    }
    
    private async Task UnsafeDecrementGroupTickets(long groupId, long amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount must be zero or more");
        await db.ExecuteAsync("UPDATE group_economy SET balance_tickets = balance_tickets - :amt WHERE group_id = :group_id",
            new
            {
                group_id = groupId,
                amt = amount,
            });
    }
    
    [Obsolete("Use the overload with a creatorType instead")]
    public async Task IncrementCurrency(long userId, CurrencyType currency, long amount)
    {
        await IncrementCurrency(CreatorType.User, userId, currency, amount);
    }
    
    public async Task IncrementCurrency(CreatorType creatorType, long creatorId, CurrencyType currency, long amount)
    {
        long newBalance;
        if (currency == CurrencyType.Robux)
        {
            if (creatorType == CreatorType.User)
            {
                await UnsafeIncrementUserRobux(creatorId, amount);
                newBalance = (await GetUserBalance(creatorId)).robux;
            }else if (creatorType == CreatorType.Group)
            {
                await UnsafeIncrementGroupRobux(creatorId, amount);
                newBalance = (await GetGroupBalance(creatorId)).robux;
            }
            else
            {
                throw new Exception("Bad creatorType");
            }
        }else if (currency == CurrencyType.Tickets)
        {
            if (creatorType == CreatorType.User)
            {
                await UnsafeIncrementUserTickets(creatorId, amount);
                newBalance = (await GetUserBalance(creatorId)).tickets;   
            }else if (creatorType == CreatorType.Group)
            {
                await UnsafeIncrementGroupTickets(creatorId, amount);
                newBalance = (await GetGroupBalance(creatorId)).tickets;
            }
            else
            {
                throw new Exception("Bad creatorType");
            }
        }
        else
        {
            throw new NotImplementedException();
        }

        if (newBalance < 0)
            throw new Exception("After increment, new balance was less than zero: " + newBalance);
    }
    
    [Obsolete("Use the overload with a creatorType instead")]
    public async Task DecrementCurrency(long userId, CurrencyType currency, long amount)
    {
        await DecrementCurrency(CreatorType.User, userId, currency, amount);
    }
    
    public async Task DecrementCurrency(CreatorType creatorType, long creatorId, CurrencyType currency, long amount)
    {
        long newBalance = -1;
        if (currency == CurrencyType.Robux)
        {
            if (creatorType == CreatorType.User)
            {
                await UnsafeDecrementUserRobux(creatorId, amount);
                newBalance = (await GetUserBalance(creatorId)).robux;
            }else if (creatorType == CreatorType.Group)
            {
                await UnsafeDecrementGroupRobux(creatorId, amount);
                newBalance = (await GetGroupBalance(creatorId)).robux;
            }
            else
            {
                throw new Exception("Bad creatorType");
            }
        }else if (currency == CurrencyType.Tickets)
        {
            if (creatorType == CreatorType.User)
            {
                await UnsafeDecrementUserTickets(creatorId, amount);
                newBalance = (await GetUserBalance(creatorId)).tickets;
            }else if (creatorType == CreatorType.Group)
            {
                await UnsafeDecrementGroupTickets(creatorId, amount);
                newBalance = (await GetGroupBalance(creatorId)).tickets;
            }
            else
            {
                throw new Exception("Bad creatorType");
            }
        }
        else
        {
            throw new NotImplementedException();
        }
        
        if (newBalance < 0)
            throw new Exception("After increment, new balance was less than zero: " + newBalance);
    }
    
    
    [Obsolete("Use method with a CreatorType instead")]
    public async Task<IEnumerable<TransactionEntryDb>> GetTransactions(long userId, PurchaseType purchaseType, int limit, int offset)
    {
        return await GetTransactions(userId, CreatorType.User, purchaseType, limit, offset);
    }

    private Tuple<SqlBuilder, SqlBuilder.Template> GetTransactionQuery()
    {
        var query = new SqlBuilder();
        var t = query.AddTemplate(
            "SELECT t.id, t.created_at as createdAt, t.user_id_two as userIdTwo, u.username, t.group_id_two as groupIdTwo, g.name as groupName, t.amount, t.currency_type as currency, t.type, t.sub_type as subType, t.asset_id as assetId, a.name as assetName, t.user_asset_id as userAssetId, t.old_username as oldUsername, t.new_username as newUsername FROM user_transaction AS t LEFT JOIN \"user\" u ON u.id = user_id_two LEFT JOIN asset a ON a.id = t.asset_id LEFT JOIN \"group\" g ON g.id = t.group_id_two /**where**/ /**orderby**/");
        return new Tuple<SqlBuilder, SqlBuilder.Template>(query, t);
    }
    
    public async Task<IEnumerable<TransactionEntryDb>> GetTransactions(long creatorId, CreatorType creatorType, PurchaseType purchaseType, int limit, int offset)
    {
        var (query, template) = GetTransactionQuery();
        query.OrderBy("t.id DESC LIMIT :limit OFFSET :offset", new
        {
            limit, offset,
        });
        if (creatorType == CreatorType.User)
        {
            query.Where("t.user_id_one = :user_id AND type = :type", new
            {
                user_id = creatorId,
                type = purchaseType,
            });
        }
        else if (creatorType == CreatorType.Group)
        {
            query.Where("t.group_id_one = :group_id AND type = :type", new
            {
                group_id = creatorId,
                type = purchaseType,
            });
        }
        else
        {
            throw new ArgumentException("Invalid creatorType");
        }
        
        return await db.QueryAsync<TransactionEntryDb>(template.RawSql, template.Parameters);
    }
    
    public async Task<IEnumerable<TransactionEntryDb>> GetTransactions(long creatorId, CreatorType creatorType, int limit, int offset)
    {
        var (query, template) = GetTransactionQuery();
        query.OrderBy("t.id DESC LIMIT :limit OFFSET :offset", new
        {
            limit, offset,
        });
        if (creatorType == CreatorType.User)
        {
            query.Where("t.user_id_one = :user_id", new
            {
                user_id = creatorId,
            });
        }
        else if (creatorType == CreatorType.Group)
        {
            query.Where("t.group_id_one = :group_id", new
            {
                group_id = creatorId,
            });
        }
        else
        {
            throw new ArgumentException("Invalid creatorType");
        }
        
        return await db.QueryAsync<TransactionEntryDb>(template.RawSql, template.Parameters);
    }

    public async Task<UserTransactionTotals> GetTransactionTotals(long userId, TimeSpan timeFrame)
    {
        var fullTime = DateTime.UtcNow.Subtract(timeFrame);
        var qu = await db.QueryAsync<UserTransactionMinimal>(
            "SELECT amount, type FROM user_transaction WHERE user_id_one = :user_id AND created_at >= :dt", new
            {
                dt = fullTime,
                user_id = userId,
            });
        var model = new UserTransactionTotals();
        foreach (var item in qu)
        {
            if (item.type == PurchaseType.Purchase)
            {
                model.outgoingRobuxTotal -= item.amount;
                model.purchaseTotal -= item.amount;
            }
            else if (item.type is PurchaseType.Sale)
            {
                model.incomingRobuxTotal += item.amount;
                model.salesTotal += item.amount;
            }
            else if (item.type is PurchaseType.BuildersClubStipend)
            {
                model.premiumStipendsTotal += item.amount;
                model.incomingRobuxTotal += item.amount;
            }
        }

        return model;
    }
    
    public async Task<UserTransactionTotals> GetGroupTransactionTotals(long groupId, TimeSpan timeFrame)
    {
        var fullTime = DateTime.UtcNow.Subtract(timeFrame);
        var qu = await db.QueryAsync<UserTransactionMinimal>(
            "SELECT amount, type FROM user_transaction WHERE group_id_one = :group_id AND created_at >= :dt", new
            {
                dt = fullTime,
                group_id = groupId,
            });
        var model = new UserTransactionTotals();
        foreach (var item in qu)
        {
            if (item.type == PurchaseType.Purchase)
            {
                model.outgoingRobuxTotal -= item.amount;
                model.purchaseTotal -= item.amount;
            }
            else if (item.type is PurchaseType.Sale)
            {
                model.incomingRobuxTotal += item.amount;
                model.salesTotal += item.amount;
            }
            else if (item.type is PurchaseType.BuildersClubStipend)
            {
                model.premiumStipendsTotal += item.amount;
                model.incomingRobuxTotal += item.amount;
            }
        }

        return model;
    }

    public async Task ChargeForAudioUpload(CreatorType creatorType, long creatorId)
    {
        await InTransaction(async _ =>
        {
            var balance = await GetBalance(creatorType, creatorId);
            if (balance.robux < 350)
                throw new LogicException(FailType.Unknown, 0, "Cannot charge user more than they own");
            await DecrementCurrency(creatorType, creatorId, CurrencyType.Robux, 350);
            await InsertTransaction(new AudioUploadTransaction(creatorType, creatorId));
            return 0;
        });
    }
    
    public async Task<long> CountTransactionsOfType(long userId, PurchaseType type, TransactionSubType subType, TimeSpan period)
    {
        var dt = DateTime.UtcNow.Subtract(period);
        var result = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT COUNT(*) as total FROM user_transaction WHERE user_id_one = :user_id AND type = :type AND sub_type = :sub_type AND created_at >= :created_at",
            new
            {
                user_id = userId,
                created_at = dt,
                type,
                sub_type = subType,
            });
        return result.total;
    }
    
    public async Task<long> CountTransactionEarningsOfType(long userId, PurchaseType type, TransactionSubType? subType, TimeSpan period)
    {
        var dt = DateTime.UtcNow.Subtract(period);
        var result = await db.QuerySingleOrDefaultAsync<Dto.Total>(
            "SELECT SUM(user_transaction.amount) AS total FROM user_transaction WHERE user_id_one = :user_id AND type = :type AND sub_type = :sub_type AND created_at >= :created_at",
            new
            {
                user_id = userId,
                created_at = dt,
                type,
                sub_type = subType,
            });
        return result.total;
    }

    public async Task<long> InsertTransaction(IEconomyTransaction trx)
    {
        var id = await db.QuerySingleOrDefaultAsync(
            "INSERT INTO user_transaction (type, currency_type, amount, user_id_one, user_id_two, asset_id, user_asset_id, sub_type, group_id_one, group_id_two, old_username, new_username) VALUES (:type, :currencyType, :amount, :userIdOne, :userIdTwo, :assetId, :userAssetId, :subType, :groupIdOne, :groupIdTwo, :oldUsername, :newUsername) RETURNING id",
            trx.GetDto());
        return (long)id.id;
    }
    
    
    public async Task InsertTransaction(params IEconomyTransaction[] transactions)
    {
        foreach (var trx in transactions)
        {
            await InsertTransaction(trx);
        }
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