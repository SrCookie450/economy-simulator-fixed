import request, { getFullUrl } from "../lib/request"

export const getMyTrades = ({ tradeType, cursor }) => {
  return request('GET', getFullUrl('trades', `/v1/trades/${tradeType}?cursor=${encodeURIComponent(cursor || '')}`)).then(d => d.data);
}

export const getTradeDetails = ({ tradeId }) => {
  return request('GET', getFullUrl('trades', `/v1/trades/${tradeId}`)).then(d => d.data);
}

export const acceptTrade = ({ tradeId }) => {
  return request('POST', getFullUrl('trades', `/v1/trades/${tradeId}/accept`)).then(d => d.data);
}

export const declineTrade = ({ tradeId }) => {
  return request('POST', getFullUrl('trades', `/v1/trades/${tradeId}/decline`)).then(d => d.data);
}

/**
 * Get the total inbound trades for the authenticated user
 * @returns {Promise<number>}
 */
export const getInboundTradeCount = () => {
  return request('GET', getFullUrl('trades', `/v1/trades/inbound/count`)).then(d => d.data.count);
}

export const createTrade = ({ offerRobux, requestRobux, offerUserAssets, requestUserAssets, offerUserId, requestUserId }) => {
  return request('POST', getFullUrl('trades', `/v1/trades/send`), {
    offers: [
      {
        robux: offerRobux,
        userAssetIds: offerUserAssets,
        userId: offerUserId,
      },
      {
        robux: requestRobux,
        userAssetIds: requestUserAssets,
        userId: requestUserId,
      }
    ]
  })
}

export const counterTrade = ({ tradeId, offerRobux, requestRobux, offerUserAssets, requestUserAssets, offerUserId, requestUserId }) => {
  return request('POST', getFullUrl('trades', `/v1/trades/${tradeId}/counter`), {
    offers: [
      {
        robux: offerRobux,
        userAssetIds: offerUserAssets,
        userId: offerUserId,
      },
      {
        robux: requestRobux,
        userAssetIds: requestUserAssets,
        userId: requestUserId,
      }
    ]
  })
}