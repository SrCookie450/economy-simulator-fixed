import { strictEqual as assert } from 'assert';

interface IWebsiteConfiguration {
	authorization: string;
	baseUrl: string;
	rccPort: number;
	port: number;
	thumbnailWebsocketPort: number;
	rcc?: string;
	content?: string;
	dockerDisabled?: boolean;
	websiteBotAuth: string;
}

const conf: Readonly<IWebsiteConfiguration> = JSON.parse(require('fs').readFileSync(require('path').join(__dirname, '../../config.json')).toString())
export default conf;

assert(typeof conf.authorization, 'string');
assert(typeof conf.baseUrl, 'string');
assert(typeof conf.rccPort, 'number');
assert(typeof conf.port, 'number');
assert(typeof conf.websiteBotAuth, 'string');
assert(typeof conf.thumbnailWebsocketPort, 'number');
if (typeof conf.rcc !== 'undefined') {
	assert(typeof conf.rcc === 'string' && conf.rcc.length > 0, true);
}