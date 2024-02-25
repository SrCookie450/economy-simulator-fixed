const ws = require('./render-ws');
const path = require('path');
const fs = require('fs');
const outDir = path.join(__dirname, './out/');
if (!fs.existsSync(outDir)) {
    fs.mkdirSync(outDir);
}
const avatar = {
    userId: 1,
    scales: {}, // ignored in r6
    bodyColors: {
        headColorId: 1,
        torsoColorId: 1,
        rightArmColorId: 1,
        leftArmColorId: 1,
        rightLegColorId: 1,
        leftLegColorId: 1,
    },
    playerAvatarType: 'R6',
    assets: [
        {
            id: 1095,
            assetType: {
                id: 19,
            }
        },
    ],
}
console.log('send');
ws('GenerateThumbnail', [avatar]).then(res => {
    console.log('ok?')
    const icon = res.data;
    // console.log('ok', icon);
    const fPath = outDir + 'player-thumbnail-gear.png'
    fs.writeFileSync(fPath, Buffer.from(icon, 'base64'));
    const open = 'file:///' + fPath.replace(/\\/g, '/');
    console.log(open);
    process.exit(0);
}).catch(e => {
    console.error('err',e)
})
console.log('uhh')