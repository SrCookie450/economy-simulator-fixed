# This script runs the integration tests in a docker container. It uses Docker to prevent messing up dev/staging environments.
docker-compose down; # reset any previous containers
docker-compose rm;
docker-compose build && docker-compose run migrations && docker-compose run integration_test;
docker-compose down;
docker-compose rm;
# docker-compose run integration_test ls "/src";