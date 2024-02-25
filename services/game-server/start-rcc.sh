#!/bin/bash
if ! [[ $(whoami) =~ "root" ]]; then
    echo "must run as root user (exiting with code 2)";
    exit 2;
fi
# Check that $1 is an integer
re='^[0-9]+$';
if ! [[ $1 =~ $re ]] ; then
   echo "error: Not a number" >&2; exit 1;
fi
content_folder="/usr/src/app/RCCService/content";
# content_folder="/home/website/game-server-v1/rcc-docker/content"
echo "Start RCC...";
#docker context use rootless && 
docker run --network="host" --cpus=1 --memory=2G --cidfile "./rcccid-$1" --add-host host.docker.internal:host-gateway rccservice -Console -Content "$content_folder" -Port "$1";