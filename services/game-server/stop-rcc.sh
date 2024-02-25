#!/bin/bash
if ! [[ $(whoami) =~ "root" ]]; then
    echo "must run as root user (exiting with code 2)";
    exit 2;
fi

docker stop "$1";
docker rm "$1";