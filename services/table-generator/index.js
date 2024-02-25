const fs = require('fs');
const path = require('path');
const apps = require('./export(2).json');
const template = fs.readFileSync('./template.html').toString('utf-8');
const xss = require('xss').filterXSS

const main = async () => {
    let entries = [];
    let added = [];
    
    for (const app of apps.filter(a => a.Role == 'Image Moderator').sort((a, b) => {
        return a['Experience'].length > b['Experience'].length ? -1 : 1;
    })) {
        let alreadyExists = added.find(a => {
            return a.About === app.About || a.Discord === app.Discord;
        });
        if (alreadyExists) continue;
        added.push(app);
        entries.push(`<tr>
        <th scope="row">${app['Auto ID']}</th>
        <td>${xss(app['Role'])}</td>
        <td><a target="_blank" href="https://example.com/search/users?keyword=${encodeURI(app['Username'])}">${xss(app['Username'])}</a></td>
        <td>${xss(app['Discord'])}</td>
        <td>${xss(app['About'], {
            allowList: {},
        })}</td>
        <td>${xss(app['Experience'], {
            allowList: {},
        })}</td>
        <td>${xss(app['Social Media'], {
            allowList: {},
        })}</td>

      </tr>`);
    }
    fs.writeFileSync('./out.html', template.replace('<!-- GOHERE -->', entries.join('')).replace('<!-- TITLE -->', `<p>Total Unique: ${entries.length.toLocaleString()}</p>`))
}
main();