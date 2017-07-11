#!/usr/bin/env bash
dotnet restore && dotnet build && dotnet pack
dotnet pack ./TCB -c Release -o ./artifacts --version-suffix=$revision
ls
cd TCB
ls
dotnet pack /p:PackageVersion=1.0.7.$TRAVIS_BUILD_NUMBER --configuration Release
cd bin
ls
cd 
Release
ls
nuget push /home/travis/build/kiksen1987/HttpCircuitBreaker/TCB/bin/Debug/Topswagcode.http.circuitbreaker.*.nupkg -ApiKey $NUGET_API_KEY -Source https://www.nuget.org -Verbosity detailed
