@ECHO OFF

:: Run unit tests 
cd test\Pivotal.Extensions.Configuration.ConfigServer.Test
dotnet test
cd ..\..

