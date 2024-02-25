import config from "./config";

const getFlag = (flag, defaultValue) => {
  const v = config.publicRuntimeConfig.backend.flags[flag];
  if (typeof v === 'undefined') return defaultValue;
  return v;
}

export default getFlag;