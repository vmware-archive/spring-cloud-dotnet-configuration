#!/bin/bash
export DOTNET_INSTALL_DIR="$PWD/.dotnetsdk"
export PATH="$DOTNET_INSTALL_DIR:$PATH"

# Run unit tests 
cd test/Pivotal.Extensions.Configuration.ConfigServer.Test
dotnet test --framework netcoreapp1.0
cd ../..

