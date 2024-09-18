rem echo off
echo

rem suppres previous publication
rmdir /s /q publish

rem publish the application

dotnet publish CodeLineCounter/CodeLineCounter.csproj -c Release -o publish -a x64 --sc true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:UseAppHost=true