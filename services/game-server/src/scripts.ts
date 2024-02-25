import fs =require('fs');
import path =require('path');

import conf from './helpers/Config';

const env = conf.baseUrl.indexOf('localhost') !== -1 ? 'development' : 'production';
console.log('Env =',env);

const scripts: Record<string, string> = {
	// GS
	gameServer: fs.readFileSync(path.join(__dirname, '../scripts/gameserver.lua')).toString(),
	// Thumbs
	playerThumbnail: fs.readFileSync(path.join(__dirname, '../scripts/player/thumbnail.lua')).toString(),
	playerHeadshot: fs.readFileSync(path.join(__dirname, '../scripts/player/headshot.lua')).toString(),
	imageTexture: fs.readFileSync(path.join(__dirname, '../scripts/asset/image.lua')).toString(),
	assetThumbnail: fs.readFileSync(path.join(__dirname, '../scripts/asset/asset.lua')).toString(),
	meshThumbnail: fs.readFileSync(path.join(__dirname, '../scripts/asset/mesh.lua')).toString(),
	headThumbnail: fs.readFileSync(path.join(__dirname, '../scripts/asset/head.lua')).toString(),
	gameThumbnail: fs.readFileSync(path.join(__dirname, '../scripts/asset/game.lua')).toString(),
}

for (const s in scripts) {
	scripts[s] = scripts[s]
		// set base url
		.replace(/local url = "base_url"/g, `local url = "${conf.baseUrl}"`)
		// set access key
		.replace(/_AUTHORIZATION_STRING_/g, conf.authorization.replace('"', '\\"'))
		.replace(/isDebugServer = false/g, env === 'development' ? 'isDebugServer = true' : 'isDebugServer = false')
		// set upload url
		// .replace(/UPLOAD_URL_HERE/g, (dockerEnabled ? 'http://host.docker.internal' : 'http://127.0.0.1:') + conf.port + '/api/upload-thumbnail-v1')
		.replace(/UPLOAD_URL_HERE/g, 'http://127.0.0.1:' + conf.port + '/api/upload-thumbnail-v1')
		.replace(/AccessKey/g, conf.authorization.replace('"', '\\"'))
		.replace(/local baseURL = "http:\/\/localhost";/g, `local baseURL = "${conf.baseUrl}";`)
}

export default () => {
	return scripts;
}