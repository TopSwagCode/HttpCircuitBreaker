#!/usr/bin/env bash
dotnet restore && dotnet build && dotnet pack

cd TCB
dotnet pack /p:PackageVersion=1.0.7.$TRAVIS_BUILD_NUMBER --configuration Release

nuget push /home/travis/build/kiksen1987/HttpCircuitBreaker/TCB/bin/Release/Topswagcode.http.circuitbreaker.*.nupkg -ApiKey $NUGET_API_KEY -Source https://www.nuget.org -Verbosity detailed
