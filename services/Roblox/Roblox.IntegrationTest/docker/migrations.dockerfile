FROM node:16-alpine

WORKDIR /migrations
# Copy everything...
COPY ["./api/migrations", "/migrations/migrations"]
COPY ["./api/knexfile.js", "/migrations/"]
COPY ["./api/package.json", "/migrations/"]
COPY ["./api/package-lock.json", "/migrations/"]
COPY ["./Roblox/Roblox.IntegrationTest/config.json", "/migrations/"]

RUN cd /migrations/ &&\
    npm ci &&\
    # required for addGroups migration
    mkdir -p /migrations/public/images;

# Run migrations, THEN run test
CMD cd /migrations/ && while true; do npx knex migrate:latest && exit 0 || sleep 1; done;