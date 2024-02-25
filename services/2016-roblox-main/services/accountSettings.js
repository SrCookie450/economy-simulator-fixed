import request, { getBaseUrl, getFullUrl } from "../lib/request"

export const getMySettingsJson = () => {
  return request('GET', getBaseUrl() + '/my/settings/json').then(d => d.data);
}

export const getMyEmail = () => {
  return request('GET', getFullUrl('accountsettings', '/v1/email')).then(d => d.data);
}

export const getInventoryPrivacy = () => {
  return request('GET', getFullUrl('accountsettings', '/v1/inventory-privacy')).then(d => d.data);
}

export const setInventoryPrivacy = ({ newPrivacy }) => {
  return request('POST', getFullUrl('accountsettings', '/v1/inventory-privacy'), {
    inventoryPrivacy: newPrivacy,
  });
}

export const getTradePrivacy = () => {
  return request('GET', getFullUrl('accountsettings', '/v1/trade-privacy')).then(d => d.data.tradePrivacy);
}

export const setTradePrivacy = ({ newPrivacy }) => {
  return request('POST', getFullUrl('accountsettings', '/v1/trade-privacy'), {
    tradePrivacy: newPrivacy,
  });
}

export const getTradeValue = () => {
  return request('GET', getFullUrl('accountsettings', '/v1/trade-value')).then(d => d.data.tradeValue);
}

export const setTradeValue = ({ newValue }) => {
  return request('POST', getFullUrl('accountsettings', '/v1/trade-value'), {
    tradeValue: newValue,
  });
}