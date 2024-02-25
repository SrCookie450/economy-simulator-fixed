module.exports = {
    apps: [
        {
            name: "GameServer",
            script: "./dist/index.js",
            instances: 1,
            watch: false,
            env: {
                "NODE_ENV": "production",
            },
            listen_timeout: 60000,
            shutdown_with_message: true,
        },
    ]
}