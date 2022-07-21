@echo off
set BUILD_TARGET=Release
set PUBLISH_PATH=D:\bin\ora-lob-unload

pushd "%~dp0"
dotnet build -c %BUILD_TARGET%
dotnet publish -o %PUBLISH_PATH% --no-build --no-self-contained -c %BUILD_TARGET%
popd

exit /b 0
