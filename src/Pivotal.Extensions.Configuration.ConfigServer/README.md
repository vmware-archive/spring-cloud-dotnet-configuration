# SCS Config Server .NET Configuration Provider

This project contains the [Spring Cloud Services - Config Server](http://docs.pivotal.io/spring-cloud-services/config-server/) client config provider.  By acting as a client to the Spring Cloud Services - Config Server, this provider enables the Config Server to become a source of configuration data for a .NET application.  You can learn more about Cloud Native Applications and the Spring Cloud Services - Config Server at [Spring Cloud Services](http://docs.pivotal.io/spring-cloud-services/index.html).

## Provider Package Name and Feeds

`Pivotal.Extensions.Configuration.ConfigServer`

[Development feed (Less Stable)](https://www.myget.org/gallery/steeltoedev) - https://www.myget.org/gallery/steeltoedev

[Master feed (Stable)](https://www.myget.org/gallery/steeltoemaster) - https://www.myget.org/gallery/steeltoemaster

[Release or Release Candidate feed](https://www.nuget.org/) - https://www.nuget.org/

## Basic Usage
You should have a good understanding of how the new .NET [Configuration model](http://docs.asp.net/en/latest/fundamentals/configuration.html) works before starting to use this provider. A basic understanding of the `ConfigurationBuilder` and how to add providers to the builder is necessary. Its also important you have a good understanding of how to setup and use a Spring Cloud Serivces - Config Server.  Detailed information on its usage can be found [here](http://docs.pivotal.io/spring-cloud-services/config-server/).

In order to retrieve configuration data from the Config Server you need to do the following:
```
1. Create and bind a service instance of the Config Server to your application.  
2. Add the Confg Server provider to the Configuration builder.
``` 
## Configure and Bind & Add Provider to Builder
You can create a Config Server instance using either the CloudFoundry command line tool (i.e. cf) or via the PCF Apps Manager. Below illustrates the command line option:
```
cf create-service p-config-server standard config-server
cf bind-service myApp config-server
cf restage myApp

```
Once you have bound the service to the app, the providers settings have been setup in `VCAP_SERVICES` and will be picked up automatically when the app is started.

Next we add the Config Server provider to the builder (e.g. `AddConfigServer()`). Here is some sample code illustrating how this is done:
```
#using Pivotal.Extensions.Configuration;
...

var builder = new ConfigurationBuilder()
    .SetBasePath(basePath)
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()                   
    .AddConfigServer();
          
var config = builder.Build();
...
```
Normally in an ASP.NET Core application, the above C# code is would be included in the constructor of the `Startup` class. For example, you might see something like this:
```
#using Pivotal.Extensions.Configuration;

public class Startup {
    .....
    public IConfigurationRoot Configuration { get; private set; }
    public Startup(IHostingEnvironment env)
    {
        // Set up configuration sources.
        var builder = new ConfigurationBuilder()
             .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .AddConfigServer();

        Configuration = builder.Build();
    }
    ....
```
## Accessing Configuration Data
Using the example code above, when the `buider.Build()` method is called, the Config Server provider will make the appropriate REST call(s) to the Config Server and retrieve configuration values based on the settings that have been provided in `appsettings.json`.

Once the configuration is built you can then access the retrieved configuration data directly from the configuration as follows:
```
....
var config = builder.Build();
var property1 = config["myconfiguration:property1"]
var property2 = config["myconfiguration:property2"] 
....
```
Alternatively you can use the [Options](https://github.com/aspnet/Options) framework together with [Dependency Injection](http://docs.asp.net/en/latest/fundamentals/dependency-injection.html) provided in ASP.NET 5 for accessing the configuration data as POCOs.
To do this, first create a POCO to represent your configuration data from the Config Server. For example:
```
public class MyConfiguration {
    public string Property1 { get; set; }
    public string Property2 { get; set; }
}
```
Then add the the following to your `public void ConfigureServices(...)` method in your `Startup` class.
```
public void ConfigureServices(IServiceCollection services)
{
    // Setup Options framework with DI
    services.AddOptions();
    
    // Configure IOptions<MyConfiguration> 
    services.Configure<MyConfiguration>(Configuration);
    ....
}
```
The `Configure<MyConfiguration>(Configuration)` binds the `myconfiguration:...` configuration values to an instance of `MyConfiguration`. After this you can then gain access to this POCO in Controllers or Views via Dependency Injection.  Here is an example controller illustrating this:
```

public class HomeController : Controller
{
    public HomeController(IOptions<MyConfiguration> myOptions)
    {
        MyOptions = myOptions.Value;
    }

    MyConfiguration MyOptions { get; private set; }

    // GET: /<controller>/
    public IActionResult Index()
    {
        ViewData["property1"] = MyOptions.Property1;
        ViewData["property2"] = MyOptions.Property2;
        return View();
    }
}
```


