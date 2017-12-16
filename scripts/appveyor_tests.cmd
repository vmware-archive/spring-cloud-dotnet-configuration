@ECHO OFF

:: Run unit tests 
cd test\Pivotal.Extensions.Configuration.ConfigServerBase.Test
dotnet restore --configfile ..\..\nuget.config
dotnet xunit -verbose
if not "%errorlevel%"=="0" goto failure
cd ..\..

cd test\Pivotal.Extensions.Configuration.ConfigServerCore.Test
dotnet restore --configfile ..\..\nuget.config
dotnet xunit -verbose
if not "%errorlevel%"=="0" goto failure
cd ..\..

cd test\Pivotal.Extensions.Configuration.Autofac.Test
dotnet restore --configfile ..\..\nuget.config
dotnet xunit -verbose
if not "%errorlevel%"=="0" goto failure
cd ..\..

echo Unit Tests Pass
goto success
:failure
echo Unit Tests Failure
exit -1
:success