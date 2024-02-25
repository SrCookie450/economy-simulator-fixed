const ws = require('ws');
const path = require('path');
// const Config = require(path.join(__dirname, '../../../services/api/config.json'));
// const authKey = encodeURIComponent(Config.gameServer.authorization);
const axios = require('axios');

module.exports = async (cmd, data) => {
    return (await axios.post('http://localhost:2958/api/public-method/', {
        method: cmd,
        arguments: data,
    }, {
        headers: {
            'roblox-server-authorization': 'hello world of deving 1234',
        }
    })).data;
}