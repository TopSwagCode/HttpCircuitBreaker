#!/usr/bin/env bash
dotnet restore && dotnet build && dotnet pack
dotnet pack ./TCB -c Release -o ./artifacts --version-suffix=$revision
./TCB/publish.sh