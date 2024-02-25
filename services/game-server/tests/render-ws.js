const ws = require('ws');
const path = require('path');
// const Config = require(path.join(__dirname, '../../../services/api/config.json'));
const authKey = encodeURIComponent('hello world of deving 1234');
/**
 * @type {ws}
 */
let client;
const setupNewClient = (cmd, commandArgs) => {
    return new Promise((res) => {
        client = new ws('http://localhost:3189?key=' + authKey);
        console.log('created client');
        client.on('open', () => {
            console.log('on open')
            client.on('message', (msg) => {
                const decoded = JSON.parse(msg.toString());
                // console.log('msg', decoded);
                res(decoded);
            });
            client.send(JSON.stringify({
                command: cmd,
                args: commandArgs,
                id: 'IntegrationTest' + new Date().getTime(),
            }));
        });
        client.on('error', (err) => {
            console.error('[error] [avatar render ws server]', err);
            process.exit(1);
        });
        client.on('close', (n, r) => {
            console.log('client close',n,r)
        })
    })
};

module.exports = async (cmd, data) => {
    return await setupNewClient(cmd, data);
}