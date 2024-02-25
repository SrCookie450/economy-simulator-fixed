const Client = require('ssh2-sftp-client');
const fs = require('fs');
const path = require('path');

/**
 * Create an SFTP client
 * @param {{host: string, password :string; username: string}} configObject 
 * @returns {Promise<import('ssh2-sftp-client')>}
 */
module.exports.createClient = async (configObject) => {
    const cl = new Client();
    await cl.connect({
        host: configObject.host,
        username: configObject.username,
        password: configObject.password,
        timeout: 60 * 1000,
    });
    return cl;
}

/**
 * Upload a file to an SFTP host
 * @param {import('ssh2-sftp-client')} client SFTP client (created with createClient())
 * @param {string} key The path to the file on SFTP
 * @param {string} filePath The absolute path of the file to upload
 */
module.exports.upload = async (client, key, filePath) => {
    // emulate S3 behaviour
    const fileName = path.basename(key);
    const dir = path.dirname(key);
    console.log('[info] upload',fileName,'to',dir);
    await client.mkdir(dir, true);
    
    await client.put(fs.createReadStream(filePath), key);
}