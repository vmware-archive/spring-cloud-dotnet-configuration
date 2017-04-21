#!/bin/bash

# Run unit tests 
cd test/Pivotal.Extensions.Configuration.ConfigServer.Test
dotnet restore --configfile ../../nuget.config
dotnet test --framework netcoreapp1.1
if [[ $? != 0 ]]; then exit 1 ; fi
cd ../..

