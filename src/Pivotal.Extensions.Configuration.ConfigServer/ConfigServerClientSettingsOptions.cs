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
    public class ConfigServerClientSettingsOptions : ST.ConfigServerClientSettingsOptions
    {
        public string Access_Token_Uri { get; set; }
        public string Client_Secret { get; set; }
        public string Client_Id { get; set; }
        public int TokenTtl { get; set; } = ConfigServerClientSettings.DEFAULT_VAULT_TOKEN_TTL;
        public int TokenRenewRate { get; set; } = ConfigServerClientSettings.DEFAULT_VAULT_TOKEN_RENEW_RATE;
        public string AccessTokenUri => Access_Token_Uri;
        public string ClientSecret => Client_Secret;
        public string ClientId => Client_Id;
        public new ConfigServerClientSettings Settings
        {
            get
            {
                ConfigServerClientSettings settings = new ConfigServerClientSettings();
                settings.Enabled = Enabled;
                settings.FailFast = FailFast;
                settings.ValidateCertificates = ValidateCertificates;
                settings.RetryAttempts = RetryAttempts;
                settings.RetryEnabled = RetryEnabled;
                settings.RetryInitialInterval = RetryInitialInterval;
                settings.RetryMaxInterval = RetryMaxInterval;
                settings.RetryMultiplier = RetryMultiplier;
                settings.Timeout = Timeout;
                settings.TokenTtl = TokenTtl;
                settings.TokenRenewRate = TokenRenewRate;
                settings.Environment = Environment;
                settings.Label = Label;
                settings.Name = Name;
                settings.Password = Password;
                settings.Uri = Uri;
                settings.Username = Username;
                settings.Token = Token;
                settings.AccessTokenUri = Access_Token_Uri;
                settings.ClientSecret = Client_Secret;
                settings.ClientId = Client_Id;
                return settings;
            }
        }
    }
}
