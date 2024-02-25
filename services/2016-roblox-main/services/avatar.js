import request from "../lib/request"
import { getFullUrl } from "../lib/request";

export const getRules = () => {
  return request('GET', getFullUrl('avatar', '/v1/avatar-rules')).then(d => d.data);
}

export const getAvatar = ({ userId }) => {
  return request('GET', getFullUrl('avatar', '/v1/users/' + userId + '/avatar')).then(d => d.data);
}

export const getMyAvatar = () => {
  return request('GET', getFullUrl('avatar', '/v1/avatar')).then(d => d.data);
}

export const redrawMyAvatar = () => {
  return request('POST', getFullUrl('avatar', '/v1/avatar/redraw-thumbnail')).then(d => d.data);
}

export const setWearingAssets = ({ assetIds }) => {
  return request('POST', getFullUrl('avatar', '/v1/avatar/set-wearing-assets'), {
    assetIds,
  });
}

export const setColors = (bodyColors) => {
  return request('POST', getFullUrl('avatar', '/v1/avatar/set-body-colors'), bodyColors);
}

export const getOutfits = ({ userId }) => {
  return request('GET', getFullUrl('avatar', '/v1/users/' + userId + '/outfits?itemsPerPage=50&page=1')).then(d => d.data);
}

export const createOutfit = ({ name }) => {
  return request('POST', getFullUrl('avatar', '/v1/outfits/create'), {
    name,
  });
}

export const wearOutfit = ({ outfitId }) => {
  return request('POST', getFullUrl('avatar', '/v1/outfits/' + outfitId + '/wear'));
}

export const deleteOutfit = ({ outfitId }) => {
  return request('POST', getFullUrl('avatar', '/v1/outfits/' + outfitId + '/delete'));
}

export const renameOutfit = ({ outfitId, name }) => {
  return request('PATCH', getFullUrl('avatar', '/v1/outfits/' + outfitId), {
    name,
  });
}
