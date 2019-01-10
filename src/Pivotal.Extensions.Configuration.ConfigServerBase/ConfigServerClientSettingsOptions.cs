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

using System;
using ST = Steeltoe.Extensions.Configuration.ConfigServer;

namespace Pivotal.Extensions.Configuration.ConfigServer
{
    [Obsolete("Use the Steeltoe.Extensions.Configuration.ConfigServerBase packages!")]
    public class ConfigServerClientSettingsOptions : ST.ConfigServerClientSettingsOptions
    {
        public new ConfigServerClientSettings Settings
        {
            get
            {
                ConfigServerClientSettings settings = new ConfigServerClientSettings
                {
                    Enabled = Enabled,
                    FailFast = FailFast,
                    ValidateCertificates = ValidateCertificates,
                    RetryAttempts = RetryAttempts,
                    RetryEnabled = RetryEnabled,
                    RetryInitialInterval = RetryInitialInterval,
                    RetryMaxInterval = RetryMaxInterval,
                    RetryMultiplier = RetryMultiplier,
                    Timeout = Timeout,
                    TokenTtl = TokenTtl,
                    TokenRenewRate = TokenRenewRate,
                    Environment = Environment,
                    Label = Label,
                    Name = Name,
                    Password = Password,
                    Uri = Uri,
                    Username = Username,
                    Token = Token,
                    AccessTokenUri = Access_Token_Uri,
                    ClientSecret = Client_Secret,
                    ClientId = Client_Id,
                    HealthEnabled = HealthEnabled,
                    HealthTimeToLive = HealthTimeToLive,
                    DiscoveryEnabled = DiscoveryEnabled,
                    DiscoveryServiceId = DiscoveryServiceId
                };
                return settings;
            }
        }
    }
}
