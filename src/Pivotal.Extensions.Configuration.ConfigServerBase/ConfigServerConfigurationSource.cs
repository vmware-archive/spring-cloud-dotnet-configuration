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
using System;
using System.Collections.Generic;
using ST = Steeltoe.Extensions.Configuration.ConfigServer;

namespace Pivotal.Extensions.Configuration.ConfigServer
{
    [Obsolete("Use the Steeltoe.Extensions.Configuration.ConfigServerBase packages!")]
    public class ConfigServerConfigurationSource : ST.ConfigServerConfigurationSource
    {
        public ConfigServerConfigurationSource(ConfigServerClientSettings defaultSettings, IList<IConfigurationSource> sources, IDictionary<string, object> properties = null, ILoggerFactory logFactory = null)
            : base(defaultSettings, sources, properties, logFactory)
        {
        }

        [Obsolete("Use the Steeltoe.Extensions.Configuration.ConfigServerBase packages!")]
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            if (Configuration == null)
            {
                // Create our own builder to build sources
                ConfigurationBuilder configBuilder = new ConfigurationBuilder();
                foreach (IConfigurationSource s in _sources)
                {
                    configBuilder.Add(s);
                }

                // Use properties provided
                foreach (var p in _properties)
                {
                    configBuilder.Properties.Add(p);
                }

                // Create configuration
                Configuration = configBuilder.Build();
            }

            return new ConfigServerConfigurationProvider(this);
        }
    }
}
