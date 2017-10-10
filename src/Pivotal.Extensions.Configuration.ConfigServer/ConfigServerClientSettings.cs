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


using ST = Steeltoe.Extensions.Configuration.ConfigServer;

namespace Pivotal.Extensions.Configuration.ConfigServer
{
    /// <summary>
    /// Holds the settings used to configure the Spring Cloud Config Server provider 
    /// <see cref="ConfigServerConfigurationProvider"/>.
    /// </summary>
    public class ConfigServerClientSettings : ST.ConfigServerClientSettings
    {
        public const int DEFAULT_VAULT_TOKEN_TTL = 300000;
        public const int DEFAULT_VAULT_TOKEN_RENEW_RATE = 60000;

        /// <summary>
        /// Default address used by provider to obtain a OAuth Access Token 
        /// </summary>
        public const string DEFAULT_ACCESS_TOKEN_URI = null;

        /// <summary>
        /// Default client id used by provider to obtain a OAuth Access Token 
        /// </summary>
        public const string DEFAULT_CLIENT_ID = null;

        /// <summary>
        /// Default client secret used by provider to obtain a OAuth Access Token 
        /// </summary>
        public const string DEFAULT_CLIENT_SECRET = null;

        /// <summary>
        /// Address used by provider to obtain a OAuth Access Token 
        /// </summary>
        public string AccessTokenUri { get; set; } = DEFAULT_ACCESS_TOKEN_URI;

        /// <summary>
        /// Client id used by provider to obtain a OAuth Access Token 
        /// </summary>
        public string ClientId { get; set; } = DEFAULT_CLIENT_ID;

        /// <summary>
        /// Client secret used by provider to obtain a OAuth Access Token 
        /// </summary>
        public string ClientSecret { get; set; } = DEFAULT_CLIENT_SECRET;

        /// <summary>
        /// Vault token Time to Live setting in Millisecoonds
        /// </summary>
        public int TokenTtl { get; set; } = DEFAULT_VAULT_TOKEN_TTL;

        /// <summary>
        /// Vault token renew rate in Milliseconds
        /// </summary>
        public int TokenRenewRate { get; set; } = DEFAULT_VAULT_TOKEN_RENEW_RATE;


        /// <summary>
        /// Initialize Config Server client settings with defaults
        /// </summary>
        public ConfigServerClientSettings() : base()
        {
        }

    }

}

