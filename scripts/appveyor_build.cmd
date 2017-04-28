:: @ECHO OFF

:: Build packages
cd %APPVEYOR_BUILD_FOLDER%
cd src\Pivotal.Extensions.Configuration.ConfigServer
dotnet restore --configfile ..\..\nuget.config
IF NOT "%APPVEYOR_REPO_TAG_NAME%"=="" (dotnet pack --configuration Release)
IF "%APPVEYOR_REPO_TAG_NAME%"=="" (dotnet pack --configuration Release --version-suffix %STEELTOE_VERSION_SUFFIX%)
cd %APPVEYOR_BUILD_FOLDER%
