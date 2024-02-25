const ws = require('./ws');
const path = require('path');
const fs = require('fs');
const outDir = path.join(__dirname, './out/');
if (!fs.existsSync(outDir)) {
    fs.mkdirSync(outDir);
}
const placeId = 139;
const gameServerId = '15c73206-80fb-4421-893b-87a9f4af20fc'; // hardcoded so we can manually find/delete test servers
const port = 53640;
ws('startGame', [placeId, gameServerId, port]).then(res => {
    console.log(res);
    process.exit(0);
});