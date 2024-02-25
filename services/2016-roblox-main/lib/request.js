import axios from 'axios';
import config from '../lib/config';
let _csrf = '';

const getFullUrl = (apiSite, fullUrl) => {
  return config.publicRuntimeConfig.backend.apiFormat.replace(/\{0\}/g, apiSite).replace(/\{1\}/g, fullUrl);
}

const getBaseUrl = () => {
  return config.publicRuntimeConfig.backend.baseUrl;
}

const getUrlWithProxy = (url) => {
  if (config.publicRuntimeConfig.backend.proxyEnabled)
    return '' + (url);
  return url;
}

const request = async (method, url, data) => {
  const isBrowser = typeof window !== 'undefined';
  try {
    let headers = {
      'x-csrf-token': _csrf,
    }
    if (!isBrowser) {
      // Auth header, if required
      const authHeaderValue = config.serverRuntimeConfig.backend.authorization;
      if (typeof authHeaderValue === 'string')
        headers[config.serverRuntimeConfig.backend.authorizationHeader || 'authorization'] = authHeaderValue;
      // Custom user agent
      headers['user-agent'] = 'Roblox2016/1.0';
    }
    const result = await axios.request({
      method,
      url: getUrlWithProxy(url),
      data: data,
      headers: headers,
      maxRedirects: 0,
    });
    return result;
  } catch (e) {
    if (e.response) {
      let resp = e.response;
      if (resp.status === 403 && resp.headers['x-csrf-token']) {
        _csrf = resp.headers['x-csrf-token'];
        return await request(method, url, data);
      }
    }
    if (isBrowser) {
      // attempt to make errors easier to diagnose
      if (e.response) {
        // check for regular
        if (e.response.data && e.response.data.errors && e.response.data.errors.length) {
          let err = e.response.data.errors[0]
          e.message = e.message + ': ' + (err.code + ': ' + err.message);
        }
      }
      throw e;
    } else {
      throw new Error(e.message);
    }
  }
}

export default request;

export {
  getFullUrl,
  getBaseUrl,
  getUrlWithProxy,
}