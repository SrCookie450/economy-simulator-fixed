import request, { getBaseUrl } from "../lib/request"
import { getFullUrl } from "../lib/request";

const baseUrl = getFullUrl('inventory', '');

export const getOwnedCopies = ({ assetId, userId }) => {
  return request('GET', `${baseUrl}/v1/users/${userId}/items/Asset/${assetId}`).then(d => d.data.data.map(v => {
    return {
      userAssetId: v.instanceId,
      seller: null,
      price: null,
      serialNumber: null,
    }
  }));
}

export const getInventory = ({ userId, limit, cursor, assetTypeId }) => {
  return request('GET', getBaseUrl() + `users/inventory/list-json?userId=${userId}&assetTypeId=${assetTypeId}&cursor=${encodeURIComponent(cursor || '')}&itemsPerPage=${limit}`).then(d => d.data);
}

export const getFavorites = ({ userId, limit, cursor, assetTypeId }) => {
  return request('GET', getBaseUrl() + `users/favorites/list-json?userId=${userId}&assetTypeId=${assetTypeId}&pageNumber=${cursor || 1}&itemsPerPage=${limit}`).then(d => d.data).then(d => {
    // we have to add cursors because roblox uses pageNumber for this endpoint.
    d.Data.nextPageCursor = cursor + 1;
    d.Data.previousPageCursor = cursor - 1;
    return d;
  });
}

export const getCollections = ({ userId }) => {
  return request('GET', getBaseUrl() + `users/profile/robloxcollections-json?userId=${userId}`).then(d => d.data.CollectionsItems)
}

export const getCollectibleInventory = ({ userId, cursor, limit, assetTypeId = 'null' }) => {
  return request('GET', getFullUrl('inventory', `/v1/users/${userId}/assets/collectibles?cursor=${encodeURIComponent(cursor || '')}&limit=${limit}&assetType=${assetTypeId}`)).then(d => d.data);
}

export const getCollectibleOwners = ({ assetId, limit, sort, cursor }) => {
  return request('GET', getFullUrl('inventory', `/v2/assets/${assetId}/owners?cursor=${encodeURIComponent(cursor || '')}&limit=${limit}&sortOrder=${sort}`)).then(d => d.data);
}