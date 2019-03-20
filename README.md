# .NET Configuration Providers

With the introduction of ASP.NET Core, Microsoft is providing a new [application configuration model](https://docs.asp.net/en/latest/fundamentals/configuration.html) for accessing configuration settings for an application. This new model supports access to key/value configuration data from a variety of different configuration providers or sources. Out of the box, ASP.NET Core comes with support for [JSON](https://github.com/aspnet/Configuration/tree/dev/src/Microsoft.Extensions.Configuration.Json), [XML](https://github.com/aspnet/Configuration/tree/dev/src/Microsoft.Extensions.Configuration.Xml) and [INI](https://github.com/aspnet/Configuration/tree/dev/src/Microsoft.Extensions.Configuration.Ini) files, as well as environment variables and command line parameters.  Additionally, Microsoft has also enabled developers to write their own [custom configuration providers](https://docs.asp.net/en/latest/fundamentals/configuration.html#custom-config-providers) should those provided by Microsoft not meet your needs.

This repo contains a custom configuration provider purpose built for CloudFoundry and Spring Cloud Services - Config Server.  The [Pivotal.Extensions.Configuration.ConfigServer](https://github.com/pivotal-cf/spring-cloud-dotnet-configuration/tree/master/src/Pivotal.Extensions.Configuration.ConfigServer) enables using the [Config Server for Pivotal Cloud Foundry](https://docs.pivotal.io/spring-cloud-services/config-server/) as a provider of configuration data.

Windows Master: [![AppVeyor Master](https://ci.appveyor.com/api/projects/status/44a6rtktwrt98xm9/branch/master?svg=true)](https://ci.appveyor.com/project/steeltoe/spring-cloud-dotnet-configuration/branch/master)

Windows Dev: [![AppVeyor Dev](https://ci.appveyor.com/api/projects/status/44a6rtktwrt98xm9/branch/dev?svg=true)](https://ci.appveyor.com/project/steeltoe/spring-cloud-dotnet-configuration/branch/dev)

Linux/OSX Master: [![Travis Master](https://travis-ci.org/pivotal-cf/spring-cloud-dotnet-configuration.svg?branch=master)](https://travis-ci.org/pivotal-cf/spring-cloud-dotnet-configuration)

Linux/OSX Dev: [![Travis Dev](https://travis-ci.org/pivotal-cf/spring-cloud-dotnet-configuration.svg?branch=dev)](https://travis-ci.org/pivotal-cf/spring-cloud-dotnet-configuration)

## .NET Runtime & Framework Support

Like the ASP.NET Core configuration providers, these providers are intended to support both .NET 4.6.1+ and .NET Core (CoreCLR/CoreFX) runtimes.  The providers are built and unit tested on Windows, Linux and OSX.

While the primary usage of the providers is intended to be with ASP.NET Core applications, they should also work fine with UWP, Console and ASP.NET 4.x apps. An ASP.NET 4.x sample app is available illustrating how this can be done.

Currently all of the code and samples have been tested on .NET Core 2.0, .NET 4.6.x, and on ASP.NET Core 2.0.0

## Usage

For more information on how to use these components see the online [Steeltoe documentation](https://steeltoe.io/).

## Nuget Feeds

All new configuration provider development is done on the dev branch. More stable versions of the providers can be found on the master branch. The latest prebuilt packages from each branch can be found on one of two MyGet feeds. Released version can be found on nuget.org.

- [Development feed (Less Stable)](https://www.myget.org/gallery/steeltoedev)
- [Master feed (Stable)](https://www.myget.org/gallery/steeltoemaster)
- [Release or Release Candidate feed](https://www.nuget.org/)

## Building Pre-requisites

To build and run the unit tests:

1. .NET Core SDK 2.0.3 or greater
1. .NET Core Runtime 2.0.3

## Building Packages & Running Tests - Windows

To build the packages on windows:

1. git clone ...
1. cd clone directory
1. cd src/project (e.g. cd src/Pivotal.Extensions.Configuration.ConfigServer)
1. dotnet restore
1. dotnet pack --configuration Release or Debug

The resulting artifacts can be found in the bin folder under the corresponding project. (e.g. src/Pivotal.Extensions.Configuration.ConfigServer/bin

To run the unit tests:

1. git clone ...
1. cd clone directory
1. cd test/test project (e.g. cd test\Pivotal.Extensions.Configuration.ConfigServer.Test)
1. dotnet restore
1. dotnet xunit -verbose

## Building Packages & Running Tests - Linux/OSX

To build the packages on Linux/OSX:

1. git clone ...
1. cd clone directory
1. cd src/project (e.g.. cd src/Pivotal.Extensions.Configuration.ConfigServer)
1. dotnet restore
1. dotnet pack --configuration Release or Debug

The resulting artifacts can be found in the bin folder under the corresponding project. (e.g. src/Pivotal.Extensions.Configuration.ConfigServer/bin

To run the unit tests:

1. git clone ...
1. cd clone directory
1. cd test\test project (e.g. cd test/Pivotal.Extensions.Configuration.ConfigServer.Test)
1. dotnet restore
1. dotnet xunit -verbose -framework netcoreapp2.0

## Sample Applications

See the [Samples](https://github.com/SteeltoeOSS/Samples) repo for examples of how to use these packages.