import getFlag from "../../lib/getFlag";
import getConfig from "next/config";
import jwt from 'jsonwebtoken';
import crypto from 'crypto';

let csrfKey;
const genKeyIfRequired = () => {
  if (!csrfKey) {
    const configKey = getConfig().serverRuntimeConfig.backend.csrfKey;
    if (configKey === undefined || typeof configKey !== 'string' || configKey.trim().length === 0) {
      console.warn('[warning] No serverRuntimeConfig.backend.csrfKey is set, so a random one will be generated for this instance. If you are running multiple instances of 2016-roblox, this may lead to problems. Set the csrfKey to a randomly generated value to remove this warning.');
      csrfKey = require('crypto').randomBytes(64).toString('base64');
    }else{
      csrfKey = configKey;
    }
  }
}
const generateCsrf = () => {
  genKeyIfRequired();
  const csrf = crypto.randomBytes(32).toString('hex');
  return {
    signed: jwt.sign({
      csrf,
      iat: Date.now(),
    }, csrfKey),
    csrf: csrf,
  }
}

const csrfValid = (req) => {
  genKeyIfRequired(); // should never be hit, but added just in case
  try {
    const csrfValue = req.headers['x-csrf-token'];
    const csrfCookie = req.cookies['.LoginCSRF'];
    if (typeof csrfValue === 'string') {
      if (typeof csrfCookie === 'string') {
        const decoded = jwt.verify(csrfCookie, csrfKey);
        if (decoded.iat > Date.now() - (300 * 1000) && decoded.csrf === csrfValue) {
          return true;
        }
      }
    }
  }catch(e) {}
  return false;
}

export default function handler(req, res) {
  if (!getFlag('requireLoginThroughCookie', true)) {
    res.status(500).json({
      success: false,
    });
    return
  }
  if (!csrfValid(req)) {
    const newCsrf = generateCsrf();
    res.setHeader('Set-Cookie', `.LoginCSRF=${newCsrf.signed}; Max-Age=${300}; Path=/; HttpOnly`);
    res.setHeader('x-csrf-token', newCsrf.csrf);
    return res.status(403).json({
      success: false,
      message: 'Invalid CSRF token',
    });
  }
    const cookie = req.body.cookie;
    if (typeof cookie !== 'string' || cookie.trim().length === 0) {
      res.status(400).json({
        success: false,
        message: 'Invalid cookie',
      });
      return;
    }
    const setCookieRequest = `.ROBLOSECURITY=${cookie}; Max-Age=${86400 * 365}; Path=/; HttpOnly; SameSite=Lax`;
    res.setHeader('Set-Cookie', setCookieRequest);
    res.status(200).json({
      success: true,
    });
}

