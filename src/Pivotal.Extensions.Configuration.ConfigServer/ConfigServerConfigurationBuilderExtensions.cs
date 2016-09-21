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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Steeltoe.Extensions.Configuration.CloudFoundry;
using Pivotal.Extensions.Configuration.ConfigServer;

namespace Pivotal.Extensions.Configuration
{
    /// <summary>
    /// Extension methods for adding <see cref="ConfigServerConfigurationProvider"/>.
    /// </summary>
    public static class ConfigServerConfigurationBuilderExtensions
    {

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
            configurationBuilder.Add(new ConfigServerConfigurationProvider(new ConfigServerClientSettings(), environment, logFactory));

            return configurationBuilder;

        }

    }
}
