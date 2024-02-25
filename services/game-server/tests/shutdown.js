const ws = require('./ws');
const path = require('path');
const fs = require('fs');
const outDir = path.join(__dirname, './out/');
if (!fs.existsSync(outDir)) {
    fs.mkdirSync(outDir);
}
const gameServerId = '8a1c6c28-84b7-4c31-970b-6f2764b57592';
ws('shutdown', [gameServerId]).then(res => {
    console.log(res);
    process.exit(0);
});