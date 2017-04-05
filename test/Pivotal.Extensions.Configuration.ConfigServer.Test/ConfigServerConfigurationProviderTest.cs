//
// Copyright 2015 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting;

namespace Pivotal.Extensions.Configuration.ConfigServer.Test
{
    public class ConfigServerConfigurationProviderTest
    {

        [Fact]
        public void SettingsConstructor__ThrowsIfSettingsNull()
        {
            // Arrange
            ConfigServerClientSettings settings = null;
            IHostingEnvironment env = new HostingEnvironment();

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new ConfigServerConfigurationProvider(settings, env));
            Assert.Contains(nameof(settings), ex.Message);
        }

        [Fact]
        public void SettingsConstructor__ThrowsIfHttpClientNull()
        {
            // Arrange
            ConfigServerClientSettings settings = new ConfigServerClientSettings();
            IHostingEnvironment env = new HostingEnvironment();
            HttpClient httpClient = null;

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new ConfigServerConfigurationProvider(settings, httpClient, env));
            Assert.Contains(nameof(httpClient), ex.Message);
        }

        [Fact]
        public void SettingsConstructor__WithLoggerFactorySucceeds()
        {
            // Arrange
            LoggerFactory logFactory = new LoggerFactory();
            IHostingEnvironment envir = new HostingEnvironment();
            ConfigServerClientSettings settings = new ConfigServerClientSettings();

            // Act and Assert
            var provider = new ConfigServerConfigurationProvider(settings, envir, logFactory);
            Assert.NotNull(provider.Logger);
        }
        [Fact]
        public void SettingsConstructor__ThrowsIfEnvironmentNull()
        {
            // Arrange
            ConfigServerClientSettings settings = new ConfigServerClientSettings();
            HttpClient httpClient = null;

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new ConfigServerConfigurationProvider(settings, httpClient, null));
            Assert.Contains(nameof(httpClient), ex.Message);
        }

        [Fact]
        public void DefaultConstructor_InitializedWithDefaultSettings()
        {
            // Arrange
            IHostingEnvironment env = new HostingEnvironment();
            ConfigServerConfigurationProvider provider = new ConfigServerConfigurationProvider(env);

            // Act and Assert
            TestHelpers.VerifyDefaults(provider.Settings);

        }

        [Fact]
        public void AddConfigServerClientSettings_ChangesDataDictionary()
        {
            // Arrange
            IHostingEnvironment env = new HostingEnvironment();
            ConfigServerClientSettings settings = new ConfigServerClientSettings();
            settings.AccessTokenUri = "http://foo.bar/";
            settings.ClientId = "client_id";
            settings.ClientSecret = "client_secret";
            settings.Enabled = true;
            settings.Environment = "environment";
            settings.FailFast = false;
            settings.Label = "label";
            settings.Name = "name";
            settings.Password = "password";
            settings.Uri = "http://foo.bar/";
            settings.Username = "username";
            settings.ValidateCertificates = false;
            ConfigServerConfigurationProvider provider = new ConfigServerConfigurationProvider(settings, env);


            // Act and Assert
            provider.AddConfigServerClientSettings();

            string value;
            Assert.True(provider.TryGet("spring:cloud:config:access_token_uri", out value));
            Assert.Equal("http://foo.bar/", value);
            Assert.True(provider.TryGet("spring:cloud:config:client_id", out value));
            Assert.Equal("client_id", value);
            Assert.True(provider.TryGet("spring:cloud:config:client_secret", out value));
            Assert.Equal("client_secret", value);
            Assert.True(provider.TryGet("spring:cloud:config:env", out value));
            Assert.Equal("environment", value);
            Assert.True(provider.TryGet("spring:cloud:config:label", out value));
            Assert.Equal("label", value);
            Assert.True(provider.TryGet("spring:cloud:config:name", out value));
            Assert.Equal("name", value);
            Assert.True(provider.TryGet("spring:cloud:config:password", out value));
            Assert.Equal("password", value);
            Assert.True(provider.TryGet("spring:cloud:config:uri", out value));
            Assert.Equal("http://foo.bar/", value);
            Assert.True(provider.TryGet("spring:cloud:config:username", out value));
            Assert.Equal("username", value);

            Assert.True(provider.TryGet("spring:cloud:config:enabled", out value));
            Assert.Equal("True", value);
            Assert.True(provider.TryGet("spring:cloud:config:failFast", out value));
            Assert.Equal("False", value);
            Assert.True(provider.TryGet("spring:cloud:config:validate_certificates", out value));
            Assert.Equal("False", value);

        }
        
        [Fact]
        public void GetConfigServerUri_WithExtraPathInfo()
        {
            // Arrange
            IHostingEnvironment env = new HostingEnvironment();
            ConfigServerClientSettings settings = new ConfigServerClientSettings() { Uri = "http://localhost:9999/myPath/path/", Name = "myName", Environment = "Production" };
            ConfigServerConfigurationProvider provider = new ConfigServerConfigurationProvider(settings, env);

            // Act and Assert
            string path = provider.GetConfigServerUri(null);
            Assert.Equal("http://localhost:9999/myPath/path/" + settings.Name + "/" + settings.Environment, path);
        }

        [Fact]
        public void GetConfigServerUri_WithExtraPathInfo_NoEndingSlash()
        {
            // Arrange
            IHostingEnvironment env = new HostingEnvironment();
            ConfigServerClientSettings settings = new ConfigServerClientSettings() { Uri = "http://localhost:9999/myPath/path", Name = "myName", Environment = "Production" };
            ConfigServerConfigurationProvider provider = new ConfigServerConfigurationProvider(settings, env);

            // Act and Assert
            string path = provider.GetConfigServerUri(null);
            Assert.Equal("http://localhost:9999/myPath/path/" + settings.Name + "/" + settings.Environment, path);
        }

        [Fact]
        public void GetConfigServerUri_NoEndingSlash()
        {
            // Arrange
            IHostingEnvironment env = new HostingEnvironment();
            ConfigServerClientSettings settings = new ConfigServerClientSettings() { Uri = "http://localhost:9999", Name = "myName", Environment = "Production" };
            ConfigServerConfigurationProvider provider = new ConfigServerConfigurationProvider(settings, env);

            // Act and Assert
            string path = provider.GetConfigServerUri(null);
            Assert.Equal("http://localhost:9999/" + settings.Name + "/" + settings.Environment, path);
        }
        [Fact]
        public void GetConfigServerUri_WithEndingSlash()
        {
            // Arrange
            IHostingEnvironment env = new HostingEnvironment();
            ConfigServerClientSettings settings = new ConfigServerClientSettings() { Uri = "http://localhost:9999/", Name = "myName", Environment = "Production" };
            ConfigServerConfigurationProvider provider = new ConfigServerConfigurationProvider(settings, env);

            // Act and Assert
            string path = provider.GetConfigServerUri(null);
            Assert.Equal("http://localhost:9999/" + settings.Name + "/" + settings.Environment, path);
        }

    }
}


