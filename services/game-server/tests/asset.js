const ws = require('./render-ws');
const path = require('path');
const fs = require('fs');
const outDir = path.join(__dirname, './out/');
if (!fs.existsSync(outDir)) {
    fs.mkdirSync(outDir);
}
const illumina = 1095;
const emp = 4263;
const skeleton = 4325;

console.log('send');
ws('GenerateThumbnailAsset', [skeleton]).then(res => {
    console.log('ok?')
    const icon = res.data;
    // console.log('ok', icon);
    const fPath = outDir + 'asset.png'
    fs.writeFileSync(fPath, Buffer.from(icon, 'base64'));
    const open = 'file:///' + fPath.replace(/\\/g, '/');
    console.log(open);
    process.exit(0);
}).catch(e => {
    console.error('err',e)
})
console.log('uhh')