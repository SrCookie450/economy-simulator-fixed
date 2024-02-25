module.exports = {
    apps: [
        {
            name: "ES Web",
            script: "./dist/index.js",
            instances: 1,
            exec_mode: "cluster",
            watch: false,
            env: {
                "NODE_ENV": "production",
            },
            listen_timeout: 60000,
            shutdown_with_message: true,
        },
        /*
        {
            name: "ES Jobs",
            script: "./dist/jobs/index.js",
            instances: 1,
            watch: false,
            env: {
                "NODE_ENV": "production",
            },
            listen_timeout: 60000,
            shutdown_with_message: true,
        },
        */
    ]
}