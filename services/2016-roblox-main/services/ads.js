import request, { getBaseUrl, getFullUrl } from "../lib/request"

export const uploadAdvertisement = ({ file, name, targetId, type }) => {
  let formData = new FormData();
  formData.append('name', name);
  formData.append('files', file);
  return request('POST', getFullUrl('ads', '/v1/user-ads/'+type+'/create?assetId=' + targetId), formData);
}


// Note: Endpoint is temporary until Roblox actually adds a "get ads" endpoint
export const getAds = ({ creatorId, creatorType }) => {
  return request('GET', getFullUrl('ads', '/v1/user-ads/'+(creatorType === 'User' ? 'User' : 'Group')+'/' + creatorId)).then(d => d.data);
}

// Note: Endpoint is temporary until Roblox adds an ads.roblox.com "buy ads" endpoint
export const bidOnAd = ({ adId, robux }) => {
  return request('POST', getFullUrl('ads', '/v1/user-ads/' + adId + '/run'), {
    robux,
  })
}