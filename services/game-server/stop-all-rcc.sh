#!/bin/bash
if ! [[ $(whoami) =~ "root" ]]; then
    echo "must run as root user (exiting with code 2)";
    exit 2;
fi

# previous command - did not free disk space
# docker kill $(docker container ls -q) || exit 0
# new command! this deletes old docker containers that we no longer need.
docker rm -vf $(docker ps -a --filter "ancestor=rccservice" -q);
# This command delets all docker images. Shouldn't really be ran from here
# docker rmi -f $(docker images -a -q);