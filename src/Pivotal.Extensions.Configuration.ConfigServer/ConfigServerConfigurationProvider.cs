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

namespace Pivotal.Extensions.Configuration.ConfigServer
{
    /// <summary>
    /// A Spring Cloud Config Server based <see cref="ConfigurationProvider" for use on CloudFoundry/>.
    /// </summary>
    public class ConfigServerConfigurationProvider : ST.ConfigServerConfigurationProvider
    {
        private const string VCAP_SERVICES_CONFIGSERVER_PREFIX = "vcap:services:p-config-server:0";

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
#if NET46
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
#if NET46
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
        private static void InitializeCloudFoundry(ConfigServerClientSettings settings, IConfigurationRoot root)
        {

            var clientConfigsection = root.GetSection(PREFIX);

            settings.Uri = ResovlePlaceholders(GetUri(clientConfigsection, root, settings.Uri), root);
            settings.AccessTokenUri = ResovlePlaceholders(GetAccessTokenUri(clientConfigsection, root), root);
            settings.ClientId = ResovlePlaceholders(GetClientId(clientConfigsection, root), root);
            settings.ClientSecret = ResovlePlaceholders(GetClientSecret(clientConfigsection, root), root);

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
    }

}

