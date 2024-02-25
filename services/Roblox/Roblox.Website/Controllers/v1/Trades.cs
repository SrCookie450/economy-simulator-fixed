using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Roblox.Dto.Trades;
using Roblox.Exceptions;
using Roblox.Logging;
using Roblox.Models;
using Roblox.Models.Trades;
using Roblox.Services;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;
using ServiceProvider = Roblox.Services.ServiceProvider;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/apisite/trades/v1")]
public class TradesControllerV1 : ControllerBase
{
    private void FeatureCheck()
    {
        FeatureFlags.FeatureCheck(FeatureFlag.EconomyEnabled, FeatureFlag.TradingEnabled);
    }
    
    [HttpGet("trades/inbound/count")]
    public async Task<dynamic> CountInboundTrades()
    {
        FeatureCheck();
        var total = await services.trades.CountInboundTrades(safeUserSession.userId);
        return new
        {
            count = total,
        };
    }

    [HttpGet("trades/{tradeType}")]
    public async Task<RobloxCollectionPaginated<dynamic>> GetUserTrades(TradeType tradeType, int limit, string? cursor)
    {
        FeatureCheck();
        var offset = cursor != null ? int.Parse(cursor) : 0;
        if (limit is > 100 or < 1) limit = 10;
        var result = (await services.trades.GetTradesOfTypeAndExpire(safeUserSession.userId, tradeType, limit, offset)).ToList();
        return new()
        {
            nextPageCursor = result.Count >= limit ? (offset + limit).ToString() : null,
            previousPageCursor = offset >= limit ? (offset - limit).ToString() : null,
            data = result.Select(c => new
            {
                id = c.id,
                user = new
                {
                    id = c.partnerId,
                    name = c.partnerUsername,
                    displayName = c.partnerUsername,
                },
                created = c.createdAt,
                expiration = c.expiresAt,
                isActive = c.status is TradeStatus.Open or TradeStatus.Pending,
                status = c.status,
            }),
        };
    }

    [HttpGet("trades/{tradeId:long}")]
    public async Task<GetTradeDataResponse> GetTradeById(long tradeId)
    {
        FeatureCheck();
        var data = await services.trades.GetTradeById(tradeId);
        if (data.userIdOne != safeUserSession.userId && data.userIdTwo != safeUserSession.userId)
            throw new BadRequestException(2, "The trade cannot be found or you are not authorized to view it");
        var items = (await services.trades.GetTradeItems(tradeId)).ToArray();
        var offers = new List<TradeOfferEntry>
        {
            // ID One
            new()
            {
                user = new ()
                {
                    id = data.userIdOne,
                    name = data.usernameOne,
                    displayName = data.usernameOne,
                },
                userAssets = items.Where(c => c.userId == data.userIdOne).Select(item => new UserAssetEntry()
                {
                    id = item.userAssetId,
                    serialNumber = item.serial,
                    assetId = item.assetId,
                    name = item.name,
                    recentAveragePrice = item.recentAveragePrice,
                    originalPrice = item.price,
                    assetStock = item.serialCount,
                    membershipType = "None",
                }),
                robux = data.userOneRobux,
            },
            // ID Two
            new()
            {
                user = new ()
                {
                    id = data.userIdTwo,
                    name = data.usernameTwo,
                    displayName = data.usernameTwo,
                },
                userAssets = items.Where(c => c.userId == data.userIdTwo).Select(item => new UserAssetEntry()
                {
                    id = item.userAssetId,
                    serialNumber = item.serial,
                    assetId = item.assetId,
                    name = item.name,
                    recentAveragePrice = item.recentAveragePrice,
                    originalPrice = item.price,
                    assetStock = item.serialCount,
                    membershipType = "None",
                }),
                robux = data.userTwoRobux,
            }
        };

        return new GetTradeDataResponse()
        {
            offers = offers,
            id = data.id,
            user = new()
            {
                id = data.userIdOne,
                name = data.usernameOne,
                displayName = data.usernameOne,
            },
            created = data.createdAt,
            expiration = data.expiresAt,
            isActive = data.status == TradeStatus.Open,
            status = data.status,
        };
    }

    [HttpPost("trades/{tradeId:long}/accept")]
    public async Task AcceptTrade(long tradeId)
    {
        FeatureCheck();
        try
        {
            await services.trades.AcceptTrade(tradeId, safeUserSession.userId);
        }
        catch (ArgumentException e)
        {
            throw new RobloxException(400, 0, e.Message);
        }
        // re-render
        var parties = await services.trades.GetTradeById(tradeId);
        Task.Run(async () =>
        {
            using var avatarService = ServiceProvider.GetOrCreate<AvatarService>();
            try
            {
                await Task.WhenAll(avatarService.RedrawAvatar(parties.userIdOne), avatarService.RedrawAvatar(parties.userIdTwo));
            }
            catch (Exception e)
            {
                Writer.Info(LogGroup.AvatarRenderThumb, "after trade background render failed: {0}\n{1}", e.Message, e.StackTrace);
            }
        });

    }

    [HttpPost("trades/{tradeId:long}/decline")]
    public async Task DeclineTrade(long tradeId)
    {
        FeatureCheck();
        await services.trades.DeclineTrade(tradeId, safeUserSession.userId);
    }

    [HttpPost("trades/send")]
    public async Task SendTrade([Required, FromBody] CreateTradeRequest request)
    {
        FeatureCheck();
        await services.trades.SendTrade(safeUserSession.userId, request.offers, true);
    }
}