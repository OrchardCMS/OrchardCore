using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// Represents the minimalistic set of fields stored for each tenant. This
    /// model is obtained from the 'IShellSettingsManager', which by default reads this
    /// from the 'App_Data/Sites/tenants.json' file. And holds the tenant 'IConfiguration'.
    /// </summary>
    public class ShellSettings : IEquatable<ShellSettings>
    {
        private IConfiguration _configuration;
        private IConfiguration _updatableData;

        public ShellSettings()
        {
        }

        public ShellSettings(ShellSettings shellSettings)
        {
            Name = shellSettings.Name;
            RequestUrlHost = shellSettings.RequestUrlHost;
            RequestUrlPrefix = shellSettings.RequestUrlPrefix;
            State = shellSettings.State;

            Configuration = shellSettings.Configuration;
            _updatableData = shellSettings._updatableData;
        }

        public string Name { get; set; }
        public string RequestUrlHost { get; set; }
        public string RequestUrlPrefix { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TenantState State { get; set; } = TenantState.Invalid;

        [JsonIgnore]
        public IConfigurationBuilder ConfigurationBuilder { get; set; } = new ConfigurationBuilder();

        /// <summary>
        /// The tenant 'IConfiguration' lazyly built from the application configuration,
        /// 'App_Data/appsettings.json' and 'App_Data/Sites/{tenant}/appsettings.json'.
        /// </summary>
        [JsonIgnore]
        public IConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    lock (this)
                    {
                        if (_configuration == null)
                        {
                            if (_updatableData == null)
                            {
                                _updatableData = new ConfigurationBuilder()
                                    .Add(new MemoryConfigurationSource())
                                    .Build();
                            }

                            _configuration = ConfigurationBuilder
                                .AddConfiguration(_updatableData)
                                .Build();
                        }
                    }
                }

                return _configuration;
            }
            set
            {
                lock (this)
                {
                    _configuration = value;
                }
            }
        }

        [JsonIgnore]
        public string this[string key]
        {
            get => Configuration[key];

            set
            {
                if (Configuration[key] != value)
                {
                    lock (this)
                    {
                        _updatableData[key] = value;
                    }
                }
            }
        }

        private StringValues StringValues => new StringValues(
            new [] { Name, RequestUrlHost, RequestUrlPrefix, State.ToString() }
            .Concat(Configuration.GetChildren().Where(s => s.Value != null)
            .OrderBy(s => s.Key).Select(s => s.Value)).ToArray());

        public bool Equals(ShellSettings other)
        {
            return StringValues.Equals(other?.StringValues ?? String.Empty);
        }

        public override int GetHashCode()
        {
            return StringValues.GetHashCode();
        }
    }
}
