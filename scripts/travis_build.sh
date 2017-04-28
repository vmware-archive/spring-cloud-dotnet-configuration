#!/bin/bash

echo Code is built in Unit Tests

cd src/Pivotal.Extensions.Configuration.ConfigServer
dotnet restore --configfile ../../nuget.config
cd ../..
