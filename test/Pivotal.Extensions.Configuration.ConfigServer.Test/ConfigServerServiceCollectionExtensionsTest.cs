using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using SteelToe.Extensions.Configuration.CloudFoundry;
using System;

using Xunit;

namespace Pivotal.Extensions.Configuration.ConfigServer.Test
{
    public class ConfigServerServiceCollectionExtensionsTest
    {
        [Fact]
        public void AddConfigServer_ThrowsIfServiceCollectionNull()
        {
            // Arrange
            IServiceCollection services = null;
            IConfigurationRoot config = null;

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => ConfigServerServiceCollectionExtensions.AddConfigServer(services, config));
            Assert.Contains(nameof(services), ex.Message);

        }
        [Fact]
        public void AddConfigServer_ThrowsIfConfigurtionNull()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IConfigurationRoot config = null;

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => ConfigServerServiceCollectionExtensions.AddConfigServer(services, config));
            Assert.Contains(nameof(config), ex.Message);

        }
        [Fact]
        public void AddConfigServer_ConfiguresConfigServerClientSettingsOptions_WithDefaults()
        {
            // Arrange
            var services = new ServiceCollection();
            var environment = new HostingEnvironment();

            // Act and Assert
            var builder = new ConfigurationBuilder().AddConfigServer(environment);
            var config = builder.Build();
            ConfigServerServiceCollectionExtensions.AddConfigServer(services, config);

            var serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetService<IOptions<ConfigServerClientSettingsOptions>>();
            Assert.NotNull(service);
            var options = service.Value;
            Assert.NotNull(options);
            TestHelpers.VerifyDefaults(options.Settings);

            Assert.Equal(ConfigServerClientSettings.DEFAULT_PROVIDER_ENABLED, options.Enabled);
            Assert.Equal(ConfigServerClientSettings.DEFAULT_FAILFAST, options.FailFast);
            Assert.Equal(ConfigServerClientSettings.DEFAULT_URI, options.Uri);
            Assert.Equal(ConfigServerClientSettings.DEFAULT_ENVIRONMENT, options.Environment);
            Assert.Equal(ConfigServerClientSettings.DEFAULT_CERTIFICATE_VALIDATION, options.ValidateCertificates);
            Assert.Null(options.Name);
            Assert.Null(options.Label);
            Assert.Null(options.Username);
            Assert.Null(options.Password);
            Assert.Null(options.AccessTokenUri);
            Assert.Null(options.ClientId);
            Assert.Null(options.ClientSecret);

        }
        [Fact]
        public void AddConfigServer_ConfiguresCloudFoundryOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var environment = new HostingEnvironment();

            // Act and Assert
            var builder = new ConfigurationBuilder().AddConfigServer(environment);
            var config = builder.Build();
            ConfigServerServiceCollectionExtensions.AddConfigServer(services, config);

            var serviceProvider = services.BuildServiceProvider();
            var app = serviceProvider.GetService<IOptions<CloudFoundryApplicationOptions>>();
            Assert.NotNull(app);
            var service = serviceProvider.GetService<IOptions<CloudFoundryServicesOptions>>();
            Assert.NotNull(service);

        }
        [Fact]
        public void AddConfigServer_AddsConfigurationAsService()
        {
            // Arrange
            var services = new ServiceCollection();
            var environment = new HostingEnvironment();

            // Act and Assert
            var builder = new ConfigurationBuilder().AddConfigServer(environment);
            var config = builder.Build();
            ConfigServerServiceCollectionExtensions.AddConfigServer(services, config);

            var service = services.BuildServiceProvider().GetService<IConfigurationRoot>();
            Assert.NotNull(service);

        }
    }
}
