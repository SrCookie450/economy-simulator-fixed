const themeType = {
  obc2016: 'obc2016',
  light: 'light',
  default: 'light',
}

const isLocalStorageAvailable = (() => {
  // @ts-ignore
  if (!process.browser) return false;
  if (typeof window === 'undefined' || !window.localStorage || !window.localStorage.getItem || !window.localStorage.setItem) return false;

  return true;
})()

const getTheme = () => {
  if (!isLocalStorageAvailable) return themeType.default;

  let value = localStorage.getItem('rbx_theme_v1');
  // validate
  if (typeof value !== 'string' || !Object.getOwnPropertyNames(themeType).includes(value)) return themeType.default;
  return themeType[value];
}

const setTheme = (themeString) => {
  if (!isLocalStorageAvailable) return;
  localStorage.setItem('rbx_theme_v1', themeString)
}

export {
  getTheme,
  setTheme,

  themeType,
}