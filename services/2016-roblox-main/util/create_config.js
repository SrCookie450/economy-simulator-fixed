const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const configPath = path.join(__dirname, '../config.json');

const main = () => {
  if (fs.existsSync(configPath)) {
    console.error('config.json already exists at this path:', configPath, '\n\nIf you want to create a fresh config, manually delete or move this file first, then run this script again.');
    return
  }
  fs.writeFileSync(configPath, JSON.stringify({
    "serverRuntimeConfig": {
      backend: {
        csrfKey: crypto.randomBytes(64).toString('base64'),
      }
    },
    "publicRuntimeConfig": {
      "backend": {
        proxyEnabled: true,
        flags: {
          myAccountPage2016Enabled: true,
          catalogGenreFilterSupported: false,
          catalogPageLimit: 28,
          catalogSaleCountVisibleFromDetailsEndpoint: false,
          catalogDetailsPageResellerLimit: 10,
          avatarPageInventoryLimit: 10,
          friendsPageLimit: 25,
          settingsPageThemeSelectorEnabled: true,
          tradeWindowInventoryCollectibleLimit: 10,
          moneyPagePromotionTabVisible: true,
          gameGenreFilterMethod: 'keyword',
          gameGenreFilterSupported: true,
          avatarPageOutfitCreatedAtAvailable: false,
          catalogDetailsPageOwnersTabEnabled: false,
        },
        "baseUrl": "https://www.roblox.com",
        "apiFormat": "https://{0}.roblox.com{1}"
      }
    },
  }));
  console.log('config.json created at this path:', configPath);
  return 0
}

main();