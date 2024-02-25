const path = require('path');
const fs = require('fs');
const configPath = path.join(__dirname, '../config.json');
const help = `flag

commands:
node util/flag testFlag enable - Enable a flag
node util/flag testFlag disable - Disable a flag`;

const getPTVerb = (op) => {
  return {
    enable: 'enabled',
    disable: 'disabled',
  }[op];
}

const main = () => {
  if (!fs.existsSync(configPath))
    throw new Error('Configuration does not exist. Please run "node util/crate_config" to create it');

  const config = JSON.parse(fs.readFileSync(configPath).toString('utf-8'));
  const flag = process.argv[2];
  const operation = process.argv[3];

  if (typeof flag !== 'string')
    throw new Error('Flag is not specified.');
  if (operation !== 'enable' && operation !== 'disable')
    throw new Error('Invalid operation: ' + operation);

  config.publicRuntimeConfig.backend.flags[flag] = operation === 'enable';
  const result = JSON.stringify(config, null, 2);
  fs.writeFileSync(configPath, result);
  console.log('Flag "' + flag + '" ' + getPTVerb(operation));
}

main();