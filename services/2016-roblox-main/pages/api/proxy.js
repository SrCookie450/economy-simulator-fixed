import axios from 'axios';
import getConfig from 'next/config';
import { fromUrl, parseDomain, ParseResultType } from 'parse-domain';
import { getBaseUrl } from '../../lib/request';
const UrlUtilities = (() => {
  const getDomainFromUrl = (url) => {
    const baseDomainParsed = parseDomain(fromUrl(url));
    if (baseDomainParsed.type === ParseResultType.Listed) {
      return baseDomainParsed.domain + '.' + baseDomainParsed.topLevelDomains.join('.');
    } else if (baseDomainParsed.type === ParseResultType.Ip) {
      return baseDomainParsed.hostname;
      console.log(baseDomainParsed.hostname)
    }else if (baseDomainParsed.type === ParseResultType.Reserved) {
      if (baseDomainParsed.hostname === 'economy-simulator.org') {
        return 'economy-simulator.org';
      }
      throw new Error('The only allowed reserved domain type is economy-simulator.org, got ' + baseDomainParsed.hostname);
    } else {
      //throw new Error('Unsupported domain type: ' + baseDomainParsed.type);
    }
  }
  const baseWithDomainAndTld = getDomainFromUrl(getBaseUrl())

  return {
    isSafe: (rawUrl) => {
      const parsedWithDomainAndTld = getDomainFromUrl(rawUrl)
      return parsedWithDomainAndTld === baseWithDomainAndTld;
    },
  }
})();

const actualHandler = async (req, res) => {
  const fullUrl = req.query.url;
  // Right now we just validate the URL. 
  // A safer approach might be to just send the parts of the URL (query params, path, api site) to this handler, then construct the correct URL here.
  const isUrlSafe = UrlUtilities.isSafe(fullUrl);// typeof fullUrl === 'string' && fullUrl.toLowerCase().startsWith(getBaseUrl())

  if (getConfig().publicRuntimeConfig.backend.proxyEnabled !== true || !isUrlSafe) {
    return res.status(500).json({
      success: false,
    });
  }
  try {
    let requestHeaders = {
      cookie: req.headers['cookie'] || '',
      'x-csrf-token': req.headers['x-csrf-token'] || '',
      'user-agent': req.headers['user-agent'],
    }
    const authHeaderValue = getConfig().serverRuntimeConfig.backend.authorization;
    if (typeof authHeaderValue === 'string')
      requestHeaders[getConfig().serverRuntimeConfig.backend.authorizationHeader || 'authorization'] = authHeaderValue;

    // TODO: whitelisted headers might be safer...
    for (const key in req.headers) {
      if (key === 'host' || key === 'connection' || key === 'accept-encoding' || key === 'host') {
        continue;
      }
      requestHeaders[key] = req.headers[key];
    }
    const result = await axios.request({
      method: req.method,
      url: fullUrl,
      data: req.body,
      maxRedirects: 0,
      headers: requestHeaders,
      validateStatus: () => true,
    });
    for (const item of Object.getOwnPropertyNames(result.headers)) {
      let value = result.headers[item];
      if (item === 'set-cookie') {
        // TODO: "localhost" needs to be configurable
        if (typeof value === 'string') {
          value = value.replace(/roblox\.com/g, 'economy-simulator.org');
        } else {
          value.forEach((v, i, arr) => {
            arr[i] = v.replace(/roblox\.com/g, 'economy-simulator.org');
          });
        }
      }
      res.setHeader(item, value);
    }
    res.status(result.status);
    res.send(result.data);
    res.end();
  } catch (e) {
    console.error(e);
    res.status(500).json({
      success: false
    })
    res.end();
  }
}

export default function handler(req, res) {
  return new Promise((resolve, reject) => {
    let chunks = []
    req.on('data', function (chunk) {
      chunks.push(chunk);
    })
    req.on('end', function () {
      req.body = Buffer.concat(chunks);
      actualHandler(req, res).then(() => resolve()).catch(e => reject(e));
    })
  })

}

export const config = {
  api: {
    bodyParser: false,
  },
}

export {
  UrlUtilities,
}