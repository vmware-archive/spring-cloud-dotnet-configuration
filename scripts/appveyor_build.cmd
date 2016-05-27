:: @ECHO OFF

:: Patch project.json files
cd %APPVEYOR_BUILD_FOLDER%\scripts
call npm install
call node patch-project-json.js ../src/Pivotal.Extensions.Configuration.ConfigServer/project.json %APPVEYOR_BUILD_VERSION% %APPVEYOR_REPO_TAG_NAME%
cd %APPVEYOR_BUILD_FOLDER%

:: Restore packages
type src\Pivotal.Extensions.Configuration.ConfigServer\project.json
cd src
dotnet restore
cd ..\test
dotnet restore
cd ..

:: Build packages
cd %APPVEYOR_BUILD_FOLDER%
cd src\Pivotal.Extensions.Configuration.ConfigServer
dotnet pack --configuration Release
cd %APPVEYOR_BUILD_FOLDER%
