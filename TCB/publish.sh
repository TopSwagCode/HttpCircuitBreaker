#!/bin/bash

PACK_COMMAND="dotnet pack --configuration Release"
PUSH_COMMAND_TEMPLATE="mono /usr/local/bin/nuget push ./bin/Release/TCB*.nupkg -ApiKey $NUGET_API_KEY -Source https://www.nuget.org -Verbosity detailed"
	
echo "(nuget API key is ${#NUGET_API_KEY} characters long)"
echo "Branch: $BRANCH"

if [ "$BRANCH" == "master" ]; then

  if [ -z "$NUGET_API_KEY" ]; then
    echo "Missing nuget API key, unable to release."
  else
    echo "This is a new release, yay!  Publishing nuget package."
	PUSH_COMMAND="${PUSH_COMMAND_TEMPLATE/$NUGET_API_KEY/$NUGET_API_KEY}"
	rm -rf obj
	rm -rf bin
	$PACK_COMMAND
    $PUSH_COMMAND
  fi

else
  echo "Not a release, skipping nuget publish."
  echo "Commands run would be:"
  echo $PACK_COMMAND
  echo $PUSH_COMMAND_TEMPLATE
fi
