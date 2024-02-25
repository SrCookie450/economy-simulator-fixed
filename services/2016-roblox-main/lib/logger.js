const logLevels = {
  development: 0,
  production: 10,
};

const logLevel = logLevels[process.env.NODE_ENV];

const groups = {
  redirects: 1,
  feed: 1,
};

const logger = {
  group: groups,
  /**
   * Info level
   * @param {keyof typeof groups} group 
   * @param {*} msg 
   * @param  {...any} args 
   * @returns 
   */
  info: (group, msg, ...args) => {
    if (typeof groups[group] === 'undefined')
      throw new Error('Undefined log group: ' + group);

    if (logLevel < groups[group]) return;
    return console.log('[logger.info]', groups[group], msg, ...args);
  },
  /**
   * Warning level, console.warn equivalent
   * @param {keyof typeof groups} group 
   * @param {*} msg 
   * @param  {...any} args 
   * @returns 
   */
  warn: (group, msg, ...args) => {
    if (typeof groups[group] === 'undefined')
      throw new Error('Undefined log group: ' + group);

    if (logLevel < groups[group]) return;
    console.warn('[logger.warn]', group, msg, ...args);
  },
  /**
   * Error level. Group is logged but group level is ignored; will always be logged regardless of level
   * @param {keyof typeof groups} group 
   * @param {*} msg 
   * @param  {...any} args 
   * @returns 
   */
  err: (group, msg, ...args) => {
    // always log errors
    console.error('[logger.err]', group, msg, ...args);
  },
}

export {
  logger,
}