import { expect } from "chai";
jest.mock('next/config', () => () => ({
  publicRuntimeConfig: {
    backend: {
      baseUrl: 'https://www.roblox.com',
    }
  }
}));

import { UrlUtilities } from "../pages/api/proxy";

describe('UrlUtilities.IsSafe()', () => {
  it('must return that a valid URL is safe', () => {
    const result = UrlUtilities.isSafe('https://www.roblox.com/test/goodUrl');
    expect(result).eq(true);
  });
  it('must return that a valid uppercase URL is safe', () => {
    const result = UrlUtilities.isSafe(('https://www.roblox.com/test/goodUrl').toUpperCase());
    expect(result).eq(true);
  });
  it('must return that a valid URL on different subdomain is safe', () => {
    const result = UrlUtilities.isSafe('https://api.roblox.com/test/goodUrl');
    expect(result).eq(true);
  });
  it('must return that a similar but invalid url is NOT safe', () => {
    for (const item of ['https://www.robloxlabs.com/test/badUrl', 'https://www.roblox.co/test/badUrl', 'https://www.roblox-com.com/test/badUrl']) {
      const result = UrlUtilities.isSafe(item);
      expect(result).eq(false);
    }
  });
});