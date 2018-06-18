// Copyright 2017 the original author or authors.
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.Linq;
using System.Reflection;

namespace Pivotal.Extensions.Configuration.ConfigServer
{
    /// <summary>
    /// Extension methods for adding <see cref="ConfigServerConfigurationProvider"/>.
    /// </summary>
    public static class ConfigServerConfigurationBuilderExtensions
    {
        private const string DEFAULT_ENVIRONMENT = "Production";

        /// <summary>
        /// Add Config Server and Cloud Foundry as application configuration sources
        /// </summary>
        /// <param name="configurationBuilder">Your <see cref="IConfigurationBuilder"/></param>
        /// <param name="logFactory">An <see cref="ILoggerFactory"/> for logging within configuration providers</param>
        /// <returns>Your <see cref="IConfigurationBuilder"/> with additional configuration providers</returns>
        public static IConfigurationBuilder AddConfigServer(this IConfigurationBuilder configurationBuilder, ILoggerFactory logFactory = null)
        {
            return configurationBuilder.AddConfigServer(DEFAULT_ENVIRONMENT, Assembly.GetEntryAssembly()?.GetName().Name, logFactory);
        }

        /// <summary>
        /// Add Config Server and Cloud Foundry as application configuration sources
        /// </summary>
        /// <param name="configurationBuilder">Your <see cref="IConfigurationBuilder"/></param>
        /// <param name="environment">The name of the environment to retrieve configuration for</param>
        /// <param name="logFactory">An <see cref="ILoggerFactory"/> for logging within configuration providers</param>
        /// <returns>Your <see cref="IConfigurationBuilder"/> with additional configuration providers</returns>
        public static IConfigurationBuilder AddConfigServer(this IConfigurationBuilder configurationBuilder, string environment, ILoggerFactory logFactory = null)
        {
            return configurationBuilder.AddConfigServer(environment, Assembly.GetEntryAssembly()?.GetName().Name, logFactory);
        }

        /// <summary>
        /// Add Config Server and Cloud Foundry as application configuration sources
        /// </summary>
        /// <param name="configurationBuilder">Your <see cref="IConfigurationBuilder"/></param>
        /// <param name="environment">The name of the environment to retrieve configuration for</param>
        /// <param name="applicationName">The name of your application, for retrieving app-specific settings</param>
        /// <param name="logFactory">An <see cref="ILoggerFactory"/> for logging within configuration providers</param>
        /// <returns>Your <see cref="IConfigurationBuilder"/> with additional configuration providers</returns>
        public static IConfigurationBuilder AddConfigServer(this IConfigurationBuilder configurationBuilder, string environment, string applicationName, ILoggerFactory logFactory = null)
        {
            if (configurationBuilder == null)
            {
                throw new ArgumentNullException(nameof(configurationBuilder));
            }

            var settings = new ConfigServerClientSettings()
            {
                Name = applicationName ?? Assembly.GetEntryAssembly()?.GetName().Name,
                Environment = environment ?? DEFAULT_ENVIRONMENT
            };

            return configurationBuilder.AddConfigServer(settings, logFactory);
        }

        /// <summary>
        /// Add Config Server and Cloud Foundry as application configuration sources
        /// </summary>
        /// <param name="configurationBuilder">Your <see cref="IConfigurationBuilder"/></param>
        /// <param name="defaultSettings">Configuration settings for accessing the Config Server</param>
        /// <param name="logFactory">An <see cref="ILoggerFactory"/> for logging within configuration providers</param>
        /// <returns>Your <see cref="IConfigurationBuilder"/> with additional configuration providers</returns>
        public static IConfigurationBuilder AddConfigServer(this IConfigurationBuilder configurationBuilder, ConfigServerClientSettings defaultSettings, ILoggerFactory logFactory = null)
        {
            if (configurationBuilder == null)
            {
                throw new ArgumentNullException(nameof(configurationBuilder));
            }

            if (defaultSettings == null)
            {
                throw new ArgumentNullException(nameof(defaultSettings));
            }

            if (!configurationBuilder.Sources.Any(c => c.GetType() == typeof(CloudFoundryConfigurationSource)))
            {
                configurationBuilder.Add(new CloudFoundryConfigurationSource());
            }

            configurationBuilder.Add(new ConfigServerConfigurationProvider(defaultSettings, logFactory));
            return configurationBuilder;
        }
    }
}
