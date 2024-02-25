module.exports = {
    apps: [
        {
            name: "AssetBackup",
            script: "./index.js",
            instances: 1,
            watch: false,
            env: {
                "NODE_ENV": "production",
            },
            restart_delay: 60 * 1000,
            shutdown_with_message: true,
        },
    ]
}