// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Steeltoe.Common.Configuration;
using Steeltoe.Common.Http;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Threading;
using STCS = Steeltoe.Extensions.Configuration.ConfigServer;

namespace Pivotal.Extensions.Configuration.ConfigServer
{
    /// <summary>
    /// A Spring Cloud Config Server based <see cref="ConfigurationProvider" for use on CloudFoundry/>.
    /// </summary>
    public class ConfigServerConfigurationProvider : STCS.ConfigServerConfigurationProvider
    {
        private const string VCAP_SERVICES_CONFIGSERVER_PREFIX = "vcap:services:p-config-server:0";
        private const string VAULT_RENEW_PATH = "vault/v1/auth/token/renew-self";
        private const string VAULT_TOKEN_HEADER = "X-Vault-Token";

        private Timer tokenRenewTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigServerConfigurationProvider"/> class.
        /// </summary>
        /// <param name="logFactory">optional logging factory</param>
        public ConfigServerConfigurationProvider(ILoggerFactory logFactory = null)
            : base(new ConfigServerClientSettings(), logFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigServerConfigurationProvider"/> class.
        /// </summary>
        /// <param name="settings">the configuration settings the provider uses when accessing the server.</param>
        /// <param name="logFactory">optional logging factory</param>
        /// </summary>
        public ConfigServerConfigurationProvider(ConfigServerClientSettings settings, ILoggerFactory logFactory = null)
            : base(settings, logFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigServerConfigurationProvider"/> class.
        /// </summary>
        /// <param name="settings">the configuration settings the provider uses when accessing the server.</param>
        /// <param name="httpClient">a HttpClient the provider uses to make requests of the server.</param>
        /// <param name="logFactory">optional logging factory</param>
        /// </summary>
        public ConfigServerConfigurationProvider(ConfigServerClientSettings settings, HttpClient httpClient, ILoggerFactory logFactory = null)
            : base(settings, httpClient, logFactory)
        {
        }

        /// <summary>
        /// Gets the configuration settings the provider uses when accessing the server.
        /// </summary>
        public new virtual ConfigServerClientSettings Settings
        {
            get
            {
                return _settings as ConfigServerClientSettings;
            }
        }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            ConfigurationBuilder config = new ConfigurationBuilder();
            foreach (IConfigurationSource s in builder.Sources)
            {
                if (s == this)
                {
                    break;
                }

                config.Add(s);
            }

            IConfigurationRoot existing = config.Build();
            STCS.ConfigurationSettingsHelper.Initialize(PREFIX, _settings, existing);
            InitializeCloudFoundry(Settings, existing);
            return this;
        }

        /// <summary>
        /// Conduct the OAuth2 client_credentials grant flow returning a task that can be used to obtain the
        /// results
        /// </summary>
        /// <returns>The task object representing asynchronous operation</returns>
        internal string FetchAccessToken()
        {
            if (string.IsNullOrEmpty(Settings.AccessTokenUri))
            {
                return null;
            }

            return HttpClientHelper.GetAccessToken(
                Settings.AccessTokenUri,
                Settings.ClientId,
                Settings.ClientSecret,
                Settings.Timeout,
                Settings.ValidateCertificates).Result;
        }

        protected internal virtual string GetVaultRenewUri()
        {
            var rawUri = Settings.RawUri;
            if (!rawUri.EndsWith("/"))
            {
                rawUri = rawUri + "/";
            }

            return rawUri + VAULT_RENEW_PATH;
        }

        protected internal virtual HttpRequestMessage GetValutRenewMessage(string requestUri)
        {
            var request = HttpClientHelper.GetRequestMessage(HttpMethod.Post, requestUri, FetchAccessToken);

            if (!string.IsNullOrEmpty(Settings.Token))
            {
                request.Headers.Add(VAULT_TOKEN_HEADER, Settings.Token);
            }

            int renewTtlSeconds = Settings.TokenTtl / 1000;
            string json = "{\"increment\":" + renewTtlSeconds.ToString() + "}";

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Content = content;
            return request;
        }

        protected internal virtual async void RefreshVaultTokenAsync(object state)
        {
            if (string.IsNullOrEmpty(Settings.Token))
            {
                return;
            }

            var obscuredToken = Settings.Token.Substring(0, 4) + "[*]" + Settings.Token.Substring(Settings.Token.Length - 4);

            // If certificate validation is disabled, inject a callback to handle properly
            SecurityProtocolType prevProtocols = (SecurityProtocolType)0;
            HttpClientHelper.ConfigureCertificateValidatation(
                _settings.ValidateCertificates,
                out prevProtocols,
                out RemoteCertificateValidationCallback prevValidator);

            HttpClient client = null;
            try
            {
                client = GetHttpClient(Settings);

                var uri = GetVaultRenewUri();
                var message = GetValutRenewMessage(uri);

                _logger?.LogInformation("Renewing Vault token {0} for {1} milliseconds at Uri {2}", obscuredToken, Settings.TokenTtl, uri);

                using (HttpResponseMessage response = await client.SendAsync(message).ConfigureAwait(false))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        _logger?.LogWarning("Renewing Vault token {0} returned status: {1}", obscuredToken, response.StatusCode);
                    }
                }
            }
            catch (Exception e)
            {
                _logger?.LogError("Unable to renew Vault token {0}. Is the token invalid or expired? - {1}", obscuredToken, e);
            }
            finally
            {
                client.Dispose();
                HttpClientHelper.RestoreCertificateValidation(_settings.ValidateCertificates, prevProtocols, prevValidator);
            }
        }

        /// <summary>
        /// Create the HttpRequestMessage that will be used in accessing the Spring Cloud Configuration server
        /// This includes obtaining a bearer token using the OAuth2 client_credentials grant flow and embedding it
        /// into the request
        /// </summary>
        /// <param name="requestUri">the Uri used when accessing the server</param>
        /// <returns>The HttpRequestMessage built from the path</returns>
        protected override HttpRequestMessage GetRequestMessage(string requestUri)
        {
            var request = HttpClientHelper.GetRequestMessage(HttpMethod.Get, requestUri, FetchAccessToken);

            if (!string.IsNullOrEmpty(Settings.Token))
            {
                RenewToken(_settings.Token);
                request.Headers.Add(TOKEN_HEADER, _settings.Token);
            }

            return request;
        }

        /// <summary>
        /// Adds the client settings for the Configuration Server to the data dictionary
        /// </summary>
        protected override void AddConfigServerClientSettings()
        {
            base.AddConfigServerClientSettings();

            Data["spring:cloud:config:access_token_uri"] = Settings.AccessTokenUri;
            Data["spring:cloud:config:client_secret"] = Settings.ClientSecret;
            Data["spring:cloud:config:client_id"] = Settings.ClientId;
            Data["spring:cloud:config:uri"] = Settings.Uri;
            Data["spring:cloud:config:tokenTtl"] = Settings.TokenTtl.ToString();
            Data["spring:cloud:config:tokenRenewRate"] = Settings.TokenRenewRate.ToString();
        }

        protected override void RenewToken(string token)
        {
            if (tokenRenewTimer == null)
            {
                tokenRenewTimer = new Timer(
                    this.RefreshVaultTokenAsync,
                    null,
                    TimeSpan.FromMilliseconds(Settings.TokenRenewRate),
                    TimeSpan.FromMilliseconds(Settings.TokenRenewRate));
            }
        }

        private static void InitializeCloudFoundry(ConfigServerClientSettings settings, IConfiguration root)
        {
            var clientConfigsection = root.GetSection(PREFIX);

            settings.Uri = GetUri(clientConfigsection, root, settings.Uri);
            settings.AccessTokenUri = GetAccessTokenUri(clientConfigsection, root);
            settings.ClientId = GetClientId(clientConfigsection, root);
            settings.ClientSecret = GetClientSecret(clientConfigsection, root);
            settings.TokenRenewRate = GetTokenRenewRate(clientConfigsection, root);
            settings.TokenTtl = GetTokenTtl(clientConfigsection, root);
        }

        private static string GetUri(IConfiguration configServerSection, IConfiguration config, string def)
        {
            var vcapConfigServerSection = config.GetSection(VCAP_SERVICES_CONFIGSERVER_PREFIX);
            return ConfigurationValuesHelper.GetSetting("credentials:uri", vcapConfigServerSection, configServerSection, config, def);
        }

        private static string GetClientSecret(IConfigurationSection configServerSection, IConfiguration config)
        {
            var vcapConfigServerSection = config.GetSection(VCAP_SERVICES_CONFIGSERVER_PREFIX);
            return ConfigurationValuesHelper.GetSetting(
                "credentials:client_secret",
                vcapConfigServerSection,
                configServerSection,
                config,
                ConfigServerClientSettings.DEFAULT_CLIENT_SECRET);
        }

        private static string GetClientId(IConfigurationSection configServerSection, IConfiguration config)
        {
            var vcapConfigServerSection = config.GetSection(VCAP_SERVICES_CONFIGSERVER_PREFIX);
            return ConfigurationValuesHelper.GetSetting(
                "credentials:client_id",
                vcapConfigServerSection,
                configServerSection,
                config,
                ConfigServerClientSettings.DEFAULT_CLIENT_ID);
        }

        private static string GetAccessTokenUri(IConfigurationSection configServerSection, IConfiguration config)
        {
            var vcapConfigServerSection = config.GetSection(VCAP_SERVICES_CONFIGSERVER_PREFIX);
            return ConfigurationValuesHelper.GetSetting(
                "credentials:access_token_uri",
                vcapConfigServerSection,
                configServerSection,
                config,
                ConfigServerClientSettings.DEFAULT_ACCESS_TOKEN_URI);
        }

        private static int GetTokenRenewRate(IConfigurationSection configServerSection, IConfiguration resolve)
        {
            return ConfigurationValuesHelper.GetInt("tokenRenewRate", configServerSection, resolve, ConfigServerClientSettings.DEFAULT_VAULT_TOKEN_RENEW_RATE);
        }

        private static int GetTokenTtl(IConfigurationSection configServerSection, IConfiguration resolve)
        {
            return ConfigurationValuesHelper.GetInt("tokenTtl", configServerSection, resolve, ConfigServerClientSettings.DEFAULT_VAULT_TOKEN_TTL);
        }
    }
}
