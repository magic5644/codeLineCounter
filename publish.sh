#!/bin/bash

# remove previous publish
rm -rf publish

# Build and publish
dotnet publish -c Release -o publish -a x64 --sc true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p UseAppHost=true