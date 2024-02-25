using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Roblox.Dto.Users;
using Roblox.Exceptions;
using Roblox.Models;
using Roblox.Models.Assets;
using Roblox.Models.Economy;
using Roblox.Models.Groups;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/economy/v2")]
public class EconomyControllerV2 : ControllerBase
{
    [HttpGet("users/{userId:long}/transaction-types")]
    public dynamic GetTransactionTypes()
    {
        return new
        {
            HasPurchase = true,
            HasSale = true,
            HasPremiumStipend = true,
            // below is always going to be false
            HasAffiliateSale = false,
            HasGroupPayout = false,
            HasCurrencyPurchase = false,
            HasTradeRobux = false,

            HasEngagementPayout = false,
            // might be enabled when groups are added (?)
            HasGroupEngagementPayout = false,
            HasAdSpend = false,
            HasDevEx = false,
            HasIndividualToGroup = false,
        };
    }

    private async Task<RobloxCollectionPaginated<dynamic>> GetTransactions(long creatorId, CreatorType creatorType, string transactionType, int limit,
        string? cursor = null)
    {
        var offset = cursor != null ? int.Parse(cursor) : 0;
        if (limit is > 100 or < 1) limit = 10;
        PurchaseType? typeId = transactionType?.ToLower() switch
        {
            "sale" => PurchaseType.Sale,
            "purchase" => PurchaseType.Purchase,
            "premiumStipend" => PurchaseType.BuildersClubStipend,
            "group-payout" => PurchaseType.GroupPayouts,
            "grouppayout" => PurchaseType.GroupPayouts,
            "grouppayouts" => PurchaseType.GroupPayouts,
            "group-payouts" => PurchaseType.GroupPayouts,
            _ => null
        };
        
        if (typeId == null || !Enum.IsDefined(typeId.Value))
            throw new BadRequestException();


        var listOfTransGenders = (await services.economy.GetTransactions(creatorId, creatorType, typeId.Value, limit, offset)).ToList();
        return new()
        {
            nextPageCursor = listOfTransGenders.Count >= limit ? (limit + offset).ToString() : null,
            previousPageCursor = offset >= limit ? (offset - limit).ToString() : null,
            data = listOfTransGenders.Select(c =>
            {
                dynamic details;
                string? typeOverride = null;
                if (c.subType is TransactionSubType.ItemPurchase or TransactionSubType.ItemResale
                    or TransactionSubType.ItemResalePurchase or TransactionSubType.ItemSale)
                {
                    details = new
                    {
                        id = c.assetId,
                        userAssetId = c.userAssetId,
                        name = c.assetName,
                        type = "Asset",
                    };
                }
                else if (c.subType is TransactionSubType.UsernameChange)
                {
                    details = new
                    {
                        name = "Username Change",
                        type = "RobloxProduct",
                    };
                }
                else if (c.subType is TransactionSubType.PositionOpen)
                {
                    details = new
                    {
                        name = "Position Open",
                        type = "RobloxProduct",
                    };
                }
                else if (c.subType is TransactionSubType.PositionClose)
                {
                    details = new
                    {
                        name = "Position Close",
                        type = "RobloxProduct",
                    };
                }
                else if (c.subType is TransactionSubType.PositionSale)
                {
                    details = new
                    {
                        name = "Position",
                        type = "RobloxProduct",
                    };
                }
                else if (c.subType is TransactionSubType.PositionPurchase)
                {
                    details = new
                    {
                        name = "Position",
                        type = "RobloxProduct",
                    };
                }
                else if (c.subType is TransactionSubType.GroupRoleSet)
                {
                    details = new
                    {
                        name = "GroupRoleSet",
                        type = "RobloxProduct",
                    };
                }
                else if (c.subType is TransactionSubType.GroupCreation)
                {
                    details = new
                    {
                        name = "Group",
                        type = "RobloxProduct",
                    };
                }
                else if (c.subType is TransactionSubType.AudioUploadLong)
                {
                    details = new
                    {
                        name = "Audio: Long Music",
                        type = "RobloxProduct",
                    };
                }
                else if (c.subType is TransactionSubType.GroupPayoutReceived)
                {
                    details = null;
                    typeOverride = "Group Revenue Payout";
                }
                else
                {
                    // TODO: Don't error, but log this somewhere so we know to add the missing subType
                    throw new Exception("Unexpected subType: " + c.subType);
                }

                return new
                {
                    c.id,
                    transactionType = typeOverride ?? c.type.ToString(),
                    created = c.createdAt,
                    isPending = false,
                    agent = new
                    {
                        // always fallback to user for legacy compat
                        id = c.groupIdTwo ?? c.userIdTwo,
                        type = c.groupIdTwo != null ? CreatorType.Group : CreatorType.User,
                        name = c.groupName ?? c.username,
                    },
                    details,
                    currency = new
                    {
                        amount = c.amount,
                        type = c.currency.ToString(),
                    },
                };
            }),
        };
    }

    [HttpGet("users/{userId:long}/transactions")]
    public async Task<RobloxCollectionPaginated<dynamic>> GetTransactions(string transactionType, int limit = 50, string? cursor = null)
    {
        return await GetTransactions(safeUserSession.userId, CreatorType.User, transactionType, limit, cursor);
    }

    [HttpGet("users/{userId:long}/transaction-totals")]
    public async Task<UserTransactionTotals> GetTransactionTotals(string timeFrame, string transactionType)
    {
        if (transactionType != "summary")
            throw new BadRequestException();
        var timeSpan = timeFrame == "day" ? TimeSpan.FromDays(1) :
            timeFrame == "week" ? TimeSpan.FromDays(7) :
            timeFrame == "month" ? TimeSpan.FromDays(30) : TimeSpan.FromDays(365);
        return await services.economy.GetTransactionTotals(safeUserSession.userId, timeSpan);
    }
    
    [HttpGet("groups/{groupId:long}/transaction-totals")]
    public async Task<UserTransactionTotals> GetTransactionGroupTotals(string timeFrame, string transactionType, long groupId)
    {
        if (transactionType != "summary")
            throw new BadRequestException();
        var timeSpan = timeFrame == "day" ? TimeSpan.FromDays(1) :
            timeFrame == "week" ? TimeSpan.FromDays(7) :
            timeFrame == "month" ? TimeSpan.FromDays(30) : TimeSpan.FromDays(365);
        
        var owner = await services.groups.GetGroupById(groupId);
        if (owner.owner == null || owner.owner.userId != safeUserSession.userId)
            throw new RobloxException(403, 0, "Forbidden");
        
        return await services.economy.GetGroupTransactionTotals(groupId, timeSpan);
    }

    [HttpGet("groups/{groupId:long}/transactions")]
    public async Task<RobloxCollectionPaginated<dynamic>> GetGroupTransactions(long groupId, string transactionType, int limit = 50, string? cursor = null)
    {
        // todo: what is the correct perm for viewing transactions?
        if (userSession == null ||
            !await services.groups.DoesUserHavePermission(userSession.userId, groupId, GroupPermission.SpendGroupFunds))
            throw new RobloxException(401, 0, "Forbidden");
        
        return await GetTransactions(groupId, CreatorType.Group, transactionType, limit, cursor);
    }

    private void FeatureCheckCurrencyExchange()
    {
        FeatureFlags.FeatureCheck(FeatureFlag.EconomyEnabled, FeatureFlag.CurrencyExchangeEnabled);
    }

    [HttpGet("currency-exchange/market/activity")]
    public async Task<dynamic> GetMarketActivity()
    {
        FeatureCheckCurrencyExchange();
        
        var robuxAverage = await services.currencyExchange.GetAverageRate(CurrencyType.Robux);
        var tixAverage = await services.currencyExchange.GetAverageRate(CurrencyType.Tickets);

        var robuxHigh = await services.currencyExchange.GetHigh(CurrencyType.Robux);
        var robuxLow = await services.currencyExchange.GetLow(CurrencyType.Robux);

        var tixHigh = await services.currencyExchange.GetHigh(CurrencyType.Tickets);
        var tixLow = await services.currencyExchange.GetLow(CurrencyType.Tickets);

        var robuxPositions = await services.currencyExchange.GetPositionsGroupByRate(CurrencyType.Robux);
        var tixPositions = await services.currencyExchange.GetPositionsGroupByRate(CurrencyType.Tickets);
        
        return new
        {
            average = new
            {
                robuxToTickets = robuxAverage,
                ticketsToRobux = tixAverage,
            },
            high = new
            {
                robuxToTickets = robuxHigh,
                ticketsToRobux = tixHigh,
            },
            low = new
            {
                robuxToTickets = robuxLow,
                ticketsToRobux = tixLow,
            },
            positions = new
            {
                robux = robuxPositions,
                tickets = tixPositions,
            }
        };
    }

    [HttpPost("currency-exchange/orders/create")]
    public async Task CreateCurrencyExchangeOrder([Required, FromBody] CreateExchangeOrderRequest request)
    {
        FeatureCheckCurrencyExchange();
        await services.currencyExchange.PlaceOrder(safeUserSession.userId, request.amount, request.desiredRate,
            request.sourceCurrency, request.isMarketOrder);
    }

    [HttpPost("currency-exchange/orders/{orderId:long}/close")]
    public async Task CloseOrder(long orderId)
    {
        FeatureCheckCurrencyExchange();
        await services.currencyExchange.CloseOrder(safeUserSession.userId, orderId);
    }

    [HttpGet("currency-exchange/orders/my/count")]
    public async Task<dynamic> CountOrdersByUser()
    {
        FeatureCheckCurrencyExchange();
        var result = await services.currencyExchange.CountPositionsByUser(safeUserSession.userId);
        return new
        {
            total = result,
        };
    }

    [HttpGet("currency-exchange/orders/my")]
    public async Task<RobloxCollection<TradeCurrencyOrder>> GetMyOrders(long startId, CurrencyType currency)
    {
        FeatureCheckCurrencyExchange();
        var result = await services.currencyExchange.GetPositionsByUser(safeUserSession.userId, currency, startId);
        return new()
        {
            data = result,
        };
    }
}