/**
 * Replace roblox.com with our url
 * @param {string} data 
 * @param {string} newUrl
 */
const replacer = (data, newUrl) => {
    let urlNoPort = newUrl;
    if (urlNoPort.indexOf(':') !== -1) {
        urlNoPort = urlNoPort.slice(0, urlNoPort.indexOf(':'));
    }
    return data.replace(/([a-z0-9]+)\.roblox\.com/g, `${newUrl}/apisite/$1`).replace(/https:\/\/localhost/g, 'http://' + urlNoPort);
}

module.exports = replacer;