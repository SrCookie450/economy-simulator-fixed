cd ../ &&
docker build --network=host --tag=rccservice -f ./docker/rccservice.dockerfile . &&
cd ./docker;