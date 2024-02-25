module.exports = {
  apps: [
    {
      name: "2016 Roblox Frontend",
      script: "npm",
      args: "run start",
      instances: 1,
      exec_mode: "cluster",
      watch: false,
      env: {
        "NODE_ENV": "production",
      },
      listen_timeout: 60000,
      shutdown_with_message: true,
    },
  ]
}