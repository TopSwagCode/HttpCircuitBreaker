#!/usr/bin/env bash
dotnet restore && dotnet build && dotnet pack
dotnet pack ./src/PROJECT_NAME -c Release -o ./artifacts --version-suffix=$revision