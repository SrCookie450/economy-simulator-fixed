const ws = require('./render-ws');
const path = require('path');
const fs = require('fs');
const outDir = path.join(__dirname, './out/');
if (!fs.existsSync(outDir)) {
    fs.mkdirSync(outDir);
}
const shirtA = 2051;

console.log('send');
ws('GenerateThumbnailTeeShirt', [shirtA,shirtA]).then(res => {
    console.log('ok?')
    const icon = res.data;
    // console.log('ok', icon);
    const fPath = outDir + 'teeshirt.png'
    fs.writeFileSync(fPath, Buffer.from(icon, 'base64'));
    const open = 'file:///' + fPath.replace(/\\/g, '/');
    console.log(open);
    process.exit(0);
}).catch(e => {
    console.error('err',e)
})
console.log('uhh')