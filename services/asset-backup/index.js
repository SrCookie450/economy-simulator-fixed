/*
Some known issues and explanations:

- I would have liked to encrypt assets, but as far as I know, there's no easy way to do encryption with rsync -
my only option would be to basically write my own version of rsync (i.e. keep track of written files on disk
and compare with that list before uploading a file).

- I would have liked to use SSH keys instead of passwords, but I didn't want to limit myself to one provider, e.g.
if other providers don't support ssh keys.

- All items are uploaded to "/home/" folder due to storage limitations
*/
const fs = require('fs');
const path = require('path');
const moment = require('moment-timezone');

const foldersToBackup = [
    {
        path: path.join(__dirname, '../api/storage/asset/'),
        name: 'asset',
    },
];

const config = JSON.parse(fs.readFileSync('./config.json').toString());

/**
     * Rsync between local computer SRC and server DEST
     * @param {{ip: string; port: number; password: string; user: string;}} server 
     * @param {string} folderSrc 
     * @param {string} folderDest 
     */
const rsync = async (server, folderSrc, folderDest) => {
    const cmd = `sshpass -p "${server.password}" rsync -a -e "ssh -o StrictHostKeyChecking=no -p ${server.port}" ${folderSrc} ${server.user}@${server.ip}:${folderDest}`;
    console.log('[info] rsync',server.ip,folderSrc,'=>',folderDest);
    return new Promise((res, rej) => {
        require('child_process').exec(cmd, (err, stdErr, stdOut) => {
            if (err) {
                return rej(err);
            }
            if (stdErr) {
                return rej(new Error(stdErr));
            }
            res(stdOut);
        });
    });
}

const backup = async () => {
    console.log('[info] backup start');
    for (const folder of foldersToBackup) {
        console.log('[info] start backup of',folder.name);
        await rsync({
            ip: config.host,
            port: config.port,
            password: config.pass,
            user: config.user,
        }, folder.path, '/home/' + folder.name + '/');
    }
    console.log('[info] backup end');
}

const main = async () => {
    const current = moment().tz('America/New_York');
    console.log('[info] backup service started -', current.format('DD MM YYYY hh:mm:ss'));
    if (!fs.existsSync('./ran_once')) {
        console.log('[info] not ran before, so create backup and restart');
        await backup();
        fs.writeFileSync('./ran_once', new Date().getTime().toString());
    }
    let daysToAdd = 0;
    let runAt = moment().tz('America/New_York').set({ hour: 0, minute: 0, second: 0, millisecond: 0 });
    while (!runAt.isAfter(current)) {
        daysToAdd++;
        runAt = moment().tz('America/New_York').add(daysToAdd, 'days').set({ hour: 0, minute: 0, second: 0, millisecond: 0 });
        console.log('[info] [while not after] add', daysToAdd, 'day' + (daysToAdd > 1 ? 's' : ''));
    }
    // milliseconds until backup should run
    let i = (runAt.unix() - current.unix()) * 1000;
    console.log('[info] running in', moment().tz('America/New_York').add(i, 'milliseconds').fromNow(true));
    setTimeout(() => {
        backup().then(() => {
            process.exit();
        })
    }, i);
}
main();