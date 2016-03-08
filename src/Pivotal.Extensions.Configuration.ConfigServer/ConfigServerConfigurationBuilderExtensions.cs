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

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SteelToe.Extensions.Configuration.CloudFoundry;

using STC = SteelToe.Extensions.Configuration;
using STCS = SteelToe.Extensions.Configuration.ConfigServer;

using Pivotal.Extensions.Configuration.ConfigServer;

namespace Pivotal.Extensions.Configuration
{
    /// <summary>
    /// Extension methods for adding <see cref="ConfigServerConfigurationProvider"/>.
    /// </summary>
    public static class ConfigServerConfigurationBuilderExtensions
    {
        private const string VCAP_SERVICES_CONFIGSERVER_PREFIX = "vcap:services:p-config-server:0";

        /// <summary>
        /// The prefix (<see cref="IConfigurationSection"/> under which all Spring Cloud Config Server 
        /// configuration settings (<see cref="ConfigServerClientSettings"/> are found. 
        ///   (e.g. spring:cloud:config:env, spring:cloud:config:uri, spring:cloud:config:enabled, etc.)
        /// </summary>
        public const string PREFIX = "spring:cloud:config";

        /// <summary>
        /// Adds the Spring Cloud Config Server provider <see cref="ConfigServerConfigurationProvider"/> 
        /// and the CloudFoundry configuration provider <see cref="CloudFoundryConfigurationProvider"/> 
        /// to <paramref name="configurationBuilder"/>.
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="environment">The hosting enviroment settings.</param>
        /// <param name="logFactory">optional logging factory. Used to enable logging in Config Server client.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        /// 
        /// Default Spring Config Server settings <see cref="ConfigServerClientSettings" /> will be used unless
        /// overriden by values found in providers previously added to the <paramref name="configurationBuilder"/>.
        /// The inclusion of the CloudFoundry configuration provider will cause configuration values to be automatically
        /// picked up from VCAP_APPLICATION, and VCAP_SERVICES and will be used when creating the settings used by the provider.
        /// Example:
        ///         var configurationBuilder = new ConfigurationBuilder();
        ///         configurationBuilder.AddJsonFile("appsettings.json")
        ///                             .AddConfigServer()
        ///                             .Build();
        ///     would use default Config Server setting unless overriden by values found in appsettings.json or
        ///     from values found in VCAP_APPLICATION and VCAP_SERVICES.
        /// </summary>
        public static IConfigurationBuilder AddConfigServer(this IConfigurationBuilder configurationBuilder, IHostingEnvironment environment, ILoggerFactory logFactory = null)
        {
            if (configurationBuilder == null)
            {
                throw new ArgumentNullException(nameof(configurationBuilder));
            }

            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }
            configurationBuilder.Add(new CloudFoundryConfigurationProvider());
            ConfigServerClientSettings settings = CreateSettings(environment, configurationBuilder.Providers, logFactory);
            configurationBuilder.Add(new ConfigServerConfigurationProvider(settings, logFactory));

            return configurationBuilder;

        }

        private static ConfigServerClientSettings CreateSettings(IHostingEnvironment environment, IEnumerable<IConfigurationProvider> providers, ILoggerFactory logFactory)
        {
            ConfigServerClientSettings settings = new ConfigServerClientSettings();

            if (providers != null)
            {
                ConfigurationRoot existing = new ConfigurationRoot(new List<IConfigurationProvider>(providers));
                STCS.ConfigurationSettingsHelper.Initialize(PREFIX, settings, environment, existing);
                InitializeCloudFoundry(settings, existing);

            }
   
            return settings;

        }

        private static void InitializeCloudFoundry(ConfigServerClientSettings settings, ConfigurationRoot root)
        {

            var clientConfigsection = root.GetSection(PREFIX);

            settings.Uri = ResovlePlaceholders(GetUri(clientConfigsection, root, settings.Uri), root);
            settings.AccessTokenUri = ResovlePlaceholders(GetAccessTokenUri(clientConfigsection, root), root);
            settings.ClientId = ResovlePlaceholders(GetClientId(clientConfigsection, root), root);
            settings.ClientSecret = ResovlePlaceholders(GetClientSecret(clientConfigsection, root), root);

        }

        private static string GetUri(IConfigurationSection configServerSection, ConfigurationRoot root, string def)
        {
        
            // Check for cloudfoundry binding vcap:services:p-config-server:0:credentials:uri
            var vcapConfigServerSection = root.GetSection(VCAP_SERVICES_CONFIGSERVER_PREFIX);
            var uri = vcapConfigServerSection["credentials:uri"];
            if (!string.IsNullOrEmpty(uri))
            {
                return uri;
            }

            // Take default if none of above
            return def;
        }


        private static string GetClientSecret(IConfigurationSection configServerSection, ConfigurationRoot root)
        {
            var vcapConfigServerSection = root.GetSection(VCAP_SERVICES_CONFIGSERVER_PREFIX);
            return GetSetting("credentials:client_secret", configServerSection, vcapConfigServerSection,
                ConfigServerClientSettings.DEFAULT_CLIENT_SECRET);

        }

        private static string GetClientId(IConfigurationSection configServerSection, ConfigurationRoot root)
        {
            var vcapConfigServerSection = root.GetSection(VCAP_SERVICES_CONFIGSERVER_PREFIX);
            return GetSetting("credentials:client_id", configServerSection, vcapConfigServerSection,
                ConfigServerClientSettings.DEFAULT_CLIENT_ID);

        }
 
        private static string GetAccessTokenUri(IConfigurationSection configServerSection, ConfigurationRoot root)
        {
            var vcapConfigServerSection = root.GetSection(VCAP_SERVICES_CONFIGSERVER_PREFIX);
            return GetSetting("credentials:access_token_uri", configServerSection, vcapConfigServerSection, 
                ConfigServerClientSettings.DEFAULT_ACCESS_TOKEN_URI);

        }

        private static string ResovlePlaceholders(string property, IConfiguration config)
        {
            return STC.PropertyPlaceholderHelper.ResovlePlaceholders(property, config);
        }

        private static string GetSetting(string key, IConfigurationSection primary, IConfigurationSection secondary, string def)
        {
            // First check for key in primary
            var setting = primary[key];
            if (!string.IsNullOrEmpty(setting))
            {
                return setting;
            }

            // Next check for key in secondary
            setting = secondary[key];
            if (!string.IsNullOrEmpty(setting))
            {
                return setting;
            }

            return def;
        }

    }
}
