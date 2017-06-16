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
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Net.Security;
using ST = Steeltoe.Extensions.Configuration.ConfigServer;
using STC = Steeltoe.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Text;

namespace Pivotal.Extensions.Configuration.ConfigServer
{
    /// <summary>
    /// A Spring Cloud Config Server based <see cref="ConfigurationProvider" for use on CloudFoundry/>.
    /// </summary>
    public class ConfigServerConfigurationProvider : ST.ConfigServerConfigurationProvider
    {
        private const string VCAP_SERVICES_CONFIGSERVER_PREFIX = "vcap:services:p-config-server:0";
        private const string VAULT_RENEW_PATH = "vault/v1/auth/token/renew-self";
        private const string VAULT_TOKEN_HEADER = "X-Vault-Token";

        private Timer tokenRenewTimer;
      

        /// <summary>
        /// Initializes a new instance of <see cref="ConfigServerConfigurationProvider"/> with default
        /// configuration settings. <see cref="ConfigServerClientSettings"/>
        /// <param name="environment">required Hosting environment, used in establishing config server profile</param>
        /// <param name="logFactory">optional logging factory</param>
        /// </summary>
        public ConfigServerConfigurationProvider(IHostingEnvironment environment, ILoggerFactory logFactory = null) :
            base(new ConfigServerClientSettings(), environment, logFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ConfigServerConfigurationProvider"/>.
        /// </summary>
        /// <param name="settings">the configuration settings the provider uses when accessing the server.</param>
        /// <param name="environment">required Hosting environment, used in establishing config server profile</param>
        /// <param name="logFactory">optional logging factory</param>
        /// </summary>
        public ConfigServerConfigurationProvider(ConfigServerClientSettings settings, IHostingEnvironment environment, ILoggerFactory logFactory = null) :
            base(settings, environment, logFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ConfigServerConfigurationProvider"/>.
        /// </summary>
        /// <param name="settings">the configuration settings the provider uses when
        /// accessing the server.</param>
        /// <param name="httpClient">a HttpClient the provider uses to make requests of
        /// the server.</param>
        /// <param name="environment">required Hosting environment, used in establishing config server profile</param>
        /// <param name="logFactory">optional logging factory</param>
        /// </summary>
        public ConfigServerConfigurationProvider(ConfigServerClientSettings settings, HttpClient httpClient, IHostingEnvironment environment, ILoggerFactory logFactory = null) :
            base(settings, httpClient, environment, logFactory)
        {

        }

        /// <summary>
        /// The configuration settings the provider uses when accessing the server.
        /// </summary>
        public new virtual ConfigServerClientSettings Settings
        {
            get
            {
                return _settings as ConfigServerClientSettings;
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
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            if (!string.IsNullOrEmpty(Settings.AccessTokenUri))
            {
                Task<string> task = GetAccessToken();
                task.Wait();

                var accessToken = task.Result;
                if (accessToken != null)
                {
                    AuthenticationHeaderValue auth = new AuthenticationHeaderValue("Bearer", accessToken);
                    request.Headers.Authorization = auth;
                }
            }

            if (!string.IsNullOrEmpty(Settings.Token))
            {
                RenewToken(_settings.Token);
                request.Headers.Add(TOKEN_HEADER, _settings.Token);
            }

            return request;
        }

        /// <summary>
        /// Conduct the  OAuth2 client_credentials grant flow returning a task that can be used to obtain the 
        /// results
        /// </summary>
        /// <returns>The task object representing asynchronous operation</returns>
        internal async Task<string> GetAccessToken()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, Settings.AccessTokenUri);
            HttpClient client = GetHttpClient(Settings);
#if NET452
            RemoteCertificateValidationCallback prevValidator = null;
            if (!Settings.ValidateCertificates)
            {
                prevValidator = ServicePointManager.ServerCertificateValidationCallback;
                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            }
#endif      

            AuthenticationHeaderValue auth = new AuthenticationHeaderValue("Basic", GetEncoded(Settings.ClientId, Settings.ClientSecret));
            request.Headers.Authorization = auth;

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            });

            try
            {
                using (client)
                {
                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            _logger?.LogInformation("Config Server returned status: {0} while obtaining access token from: {1}",
                                response.StatusCode, Settings.AccessTokenUri);
                            return null;
                        }

                        var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
                        var token = payload.Value<string>("access_token");
                        return token;
                    }
                }
            }
            catch (Exception e)
            {
                _logger?.LogError("Config Server exception: {0} ,obtaining access token from: {1}", e, Settings.AccessTokenUri);
            }
#if NET452
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback = prevValidator;
            }
#endif
            return null;
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
            ST.ConfigurationSettingsHelper.Initialize(PREFIX, _settings, _environment, existing);
            InitializeCloudFoundry(Settings, existing);
            return this;
        }

        protected override void RenewToken(string token)
        {
            if (tokenRenewTimer == null)
            {
                tokenRenewTimer = new Timer(this.RefreshVaultTokenAsync, null, 
                    TimeSpan.FromMilliseconds(Settings.TokenRenewRate), TimeSpan.FromMilliseconds(Settings.TokenRenewRate));
            }
        }


        internal protected virtual string GetVaultRenewUri()
        {
            var rawUri = Settings.RawUri;
            if (!rawUri.EndsWith("/"))
                rawUri = rawUri + "/";

            return rawUri + VAULT_RENEW_PATH;

        }

        internal protected virtual HttpRequestMessage GetValutRenewMessage(string requestUri)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            if (!string.IsNullOrEmpty(Settings.AccessTokenUri))
            {
                Task<string> task = GetAccessToken();
                task.Wait();

                var accessToken = task.Result;
                if (accessToken != null)
                {
                    AuthenticationHeaderValue auth = new AuthenticationHeaderValue("Bearer", accessToken);
                    request.Headers.Authorization = auth;
                }
            }
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



        internal protected virtual async void RefreshVaultTokenAsync(object state)
        {
            if (string.IsNullOrEmpty(Settings.Token))
                return;

            var obscuredToken = Settings.Token.Substring(0, 4) + "[*]" + Settings.Token.Substring(Settings.Token.Length - 4);
#if NET452
            RemoteCertificateValidationCallback prevValidator = null;
            if (!Settings.ValidateCertificates)
            {
                prevValidator = ServicePointManager.ServerCertificateValidationCallback;
                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            }
#endif      
            HttpClient client = null;
            try
            {
                client = GetHttpClient(Settings);

                var uri = GetVaultRenewUri();
                var message = GetValutRenewMessage(uri);
              

                _logger?.LogInformation("Renewing Vault token {0} for {1} milliseconds at Uri {2}", obscuredToken, Settings.TokenTtl, uri);

                using (HttpResponseMessage response = await client.SendAsync(message))
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

            } finally
            {
                client.Dispose();
#if NET452
                ServicePointManager.ServerCertificateValidationCallback = prevValidator;
#endif
            }
        }


        private static void InitializeCloudFoundry(ConfigServerClientSettings settings, IConfigurationRoot root)
        {

            var clientConfigsection = root.GetSection(PREFIX);

            settings.Uri = ResovlePlaceholders(GetUri(clientConfigsection, root, settings.Uri), root);
            settings.AccessTokenUri = ResovlePlaceholders(GetAccessTokenUri(clientConfigsection, root), root);
            settings.ClientId = ResovlePlaceholders(GetClientId(clientConfigsection, root), root);
            settings.ClientSecret = ResovlePlaceholders(GetClientSecret(clientConfigsection, root), root);
            settings.TokenRenewRate = GetTokenRenewRate(clientConfigsection);
            settings.TokenTtl = GetTokenTtl(clientConfigsection);
        }

        private static string GetUri(IConfigurationSection configServerSection, IConfigurationRoot root, string def)
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

        private static int GetTokenRenewRate(IConfigurationSection configServerSection)
        {
            return GetInt("tokenRenewRate", configServerSection,  ConfigServerClientSettings.DEFAULT_VAULT_TOKEN_RENEW_RATE);

        }

        private static int GetTokenTtl(IConfigurationSection configServerSection)
        {
            return GetInt("tokenTtl", configServerSection, ConfigServerClientSettings.DEFAULT_VAULT_TOKEN_TTL);

        }

        private static string GetClientSecret(IConfigurationSection configServerSection, IConfigurationRoot root)
        {
            var vcapConfigServerSection = root.GetSection(VCAP_SERVICES_CONFIGSERVER_PREFIX);
            return GetSetting("credentials:client_secret", configServerSection, vcapConfigServerSection,
                ConfigServerClientSettings.DEFAULT_CLIENT_SECRET);

        }

        private static string GetClientId(IConfigurationSection configServerSection, IConfigurationRoot root)
        {
            var vcapConfigServerSection = root.GetSection(VCAP_SERVICES_CONFIGSERVER_PREFIX);
            return GetSetting("credentials:client_id", configServerSection, vcapConfigServerSection,
                ConfigServerClientSettings.DEFAULT_CLIENT_ID);

        }

        private static string GetAccessTokenUri(IConfigurationSection configServerSection, IConfigurationRoot root)
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

        private static int GetInt(string key, IConfigurationSection clientConfigsection, int def)
        {
            var val = clientConfigsection[key];
            if (!string.IsNullOrEmpty(val))
            {
                int result;
                if (int.TryParse(val, out result))
                    return result;
            }
            return def;
        }
    }

}

