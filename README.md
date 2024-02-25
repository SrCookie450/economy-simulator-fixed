1. setup ubuntu and redis

go to powershell as administrator, then do ```wsl --install``` if you get erorrs enable "Windows Subsystem For Linux" and "Windows Virtual Machine" in the windows features.

Once it's done, restart your computer then ubuntu will open, let it install. itll ask for a uixd name enter a name such as ecs or anythng. then a password. 

Do ```sudo apt update``` then after its done do ```sudo apt install redis``` and press y when it asks, when its done do ```redis-server``` now keep this open.

2. get apps

node.js: ```https://nodejs.org/dist/v18.16.1/node-v18.16.1-x64.msi```

postgreSQL: ```https://sbp.enterprisedb.com/getfile.jsp?fileid=1258627```

Dotnet 6: ```https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.412-windows-x64-installer```

Go lang: ```https://go.dev/dl/go1.20.6.windows-amd64.msi```

3. install

Press next until it asks for a password, make sure you remember this. then keep pressing next, wait for it to install. Then install node.js then dotnet 6 then go lang.

4. setting up 

go to ```services/api``` and make a file named ```config.json``` and paste this code into it
```
{
    "knex": {
	"client": "pg",
        "connection": {
        "host": "127.0.0.1",
        "user": "postgres",
        "password": "password",
        "database": "postgres"
        }
    }
}
```

now go to the command in the ```services/api``` folder and paste this ```npm i``` which installs node modules, then do npx knex migrate:latest which installs the database tables and shit.

now you have to go to ```services/Roblox/Roblox.Website``` open the appsettings.example.json file, and rename to appsettings.json and then go into it. go to line 37 and change the line ```    "OwnerUserId": "3",``` to ```    "OwnerUserId": "1",```

go to line 26 now, and change the line ```"Postgres": "Host=127.0.0.1; Database=economysimulator; Password=test; Username=postgres; Maximum Pool Size=20",``` to 

``` "Postgres": "Host=127.0.0.1; Database=postgres; Password=password; Username=postgres; Maximum Pool Size=20",```

Now press ```ctrl + h``` and change C:\\Users\\mark\\Desktop\\ to C:\\Users\\yourusername\\Downloads\\``` FOR EVERY SINGLE ONE, MAKE SURE THE FOLDER IS NAMED ECS AND IN DOWNLOADS

now appsettings are done.

Go into ```services/api``` create a folder named ```storage``` inside that folder make one named ```asset``` then go to ```services/api/public/images``` make a folder named ```thumbnails``` and ```group```

go to ```services/Roblox/Roblox.Website``` and in command prompt do ```dotnet run```

go to ```services/admin``` and run ```npm i``` and ```npm run build``` in a command.

go to ```services/2016-roblox-main``` and create a file named ```config.json```

paste this into it

```
{
  "serverRuntimeConfig": {
    "backend": {"csrfKey":"g0qiiDZw7jM2l54+7qsuRaymx6nBGdCKT9Kc0bqJB3aZ26rSsPMXfg8uWfUBtTqWenDVy+AQS1jkdrgvUwVSsw=="}
  },
  "publicRuntimeConfig": {
    "backend": {
      "proxyEnabled": true,
      "flags": {
        "myAccountPage2016Enabled": true,
        "catalogGenreFilterSupported": true,
        "catalogPageLimit": 28,
        "catalogSaleCountVisibleFromDetailsEndpoint": true,
        "commentsEndpointHasAreCommentsDisabledProp": true,
        "catalogDetailsPageResellerLimit": 10,
        "avatarPageInventoryLimit": 10,
        "friendsPageLimit": 25,
        "settingsPageThemeSelectorEnabled": true,
        "tradeWindowInventoryCollectibleLimit": 10,
        "moneyPagePromotionTabVisible": true,
        "gameGenreFilterSupported": true,
        "avatarPageOutfitCreatedAtAvailable": true,
        "catalogDetailsPageOwnersTabEnabled": true,
        "launchUsingEsURI": true
      },
      "baseUrl": "https://your.domain/",
      "apiFormat": "https://your.domain/apisite/{0}{1}"
    }
  }
}
```

replacing your.domain with your domain.

now in a command in the same folder do ```npm i``` and ```npm run build``` then ```npm run start```

now go to ```services/AssetValidationServiceV2``` and in command do ```go run main.go``` 

now go to ```services/RCCService``` and in command do ```RCCService.exe -console -placeid:1818```

now go to ```services/game-server``` and make a file named config.json in the file paste this
```
{
    "rcc": "C:\\Users\\yourusername\\Downloads\\ECS\\services\\RCCService",
    "authorization": "90WGEGNJGWHIWGOI31900H9GIOGI",
    "baseUrl": "http://localhost:5000",
    "rccPort": 64989,
    "port": 3040,
    "websiteBotAuth": "UW8U8TU9W9R8RHGRJOGWGOINOOWGNWRNJWWNRJ",
    "thumbnailWebsocketPort": 3189,
    "dockerDisabled": true
}
```

now this gets a little complicated, go into ```services/game-server/scripts```


in gameserver.lua press ```ctrl + h``` and replace ```economy-simulator.org``` with your domain, then go back to scripts, go into ```player``` folder and each file do the replacing, then in asset folders files. make sure to not replace the http and https

now go back into game-server and run ```npm i``` ```npm run build``` and ```npm run start```

step 6. cloudflare tunneling

Ok, so we got the whole site setup, exit out of all the cmd tabs, and go to https://cloudflare.com and sign up, add your domain onto the site. 

then go into the domain, and on the side bar press ```traffic``` then press ```cloudflare tunnel``` then launch zero trust dashboard sign up for the free option if it asks you to do that.

then press your email.

on the sidebar press ```access``` then ```tunnels``` then create a tunnel, name it and press save tunnel.

then download https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.msi and run the power shell as administrator.

it says ```run the following command``` copy that and paste it into the powershell and scroll down on cloudflare wait for connectors to show your connected and press next.

on domain put your domain and type put http and url put ```localhost:5000``` and press save hostname.

Step 7. 

congrats site is setup go into ```/services``` on the source and run ```runall.bat``` and then when its all done go to your site, sign up the ROBLOX account, make sure its ID 1 and is all caps, then go to https://your.domain/admin and go to create player, put id 2 and name UGC and a random password, then go to the user on admin panel and nullify password.

now go back to create and id 12 name BadDecisions make sure its a hard pass.

now you can sign up.

congrats site made

- Made by Deadly
