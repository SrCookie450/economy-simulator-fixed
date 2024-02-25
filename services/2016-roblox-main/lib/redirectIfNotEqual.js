/**
 * Redirect if the current URL does not match the expectedUrl
 * @param {string} expectedUrl 
 * @returns True if redirect is starting, false otherwise
 */
const redirectIfNotEqual = (expectedUrl) => {
  if (!expectedUrl.startsWith('/')) throw new Error('expectedUrl must start with a backslash');
  let base = window.location.protocol + '//' + window.location.host;
  let currentUrl = window.location.href;
  expectedUrl = base + expectedUrl;
  if (currentUrl !== expectedUrl) {
    window.location.href = expectedUrl;
    return true;
  }
  return false;
}

export default redirectIfNotEqual;