#!/usr/bin/env bash
dotnet restore && dotnet build && dotnet pack
dotnet pack ./TCB -c Release -o ./artifacts --version-suffix=$revision
ls
cd TCB
ls
dotnet pack --configuration Release
nuget push ./bin/Release/Topswagcode.http.circuitbreaker.1.0.0.nupkg -ApiKey $NUGET_API_KEY -Source https://www.nuget.org -Verbosity detailed
