#!/bin/bash
if ! [[ $(whoami) =~ "root" ]]; then
    echo "must run as root user (exiting with code 2)";
    exit 2;
fi

#--mount type=bind,source="$(pwd)"/target,target=/app
docker run --network="host" --cpus=1 --memory=2G --mount type=bind,source="$(pwd)"/place-conversion-data,target=/app/place-conversion-data rccservice "$1"