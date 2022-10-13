#!/bin/bash

# This bash script will download Oracle docker images from github and builds the container image with bash script
# Oracle database version 18.4.0. You need to run this if you want to run the tests.

git clone https://github.com/oracle/docker-images.git ./_build/docker-images
cd ./_build/docker-images/OracleDatabase/SingleInstance/dockerfiles
./buildContainerImage.sh -v 18.4.0 -x

docker-compose -f ../../../../../Frends.Oracle.ExecuteProcedure.Tests/docker-compose.yml up -d

