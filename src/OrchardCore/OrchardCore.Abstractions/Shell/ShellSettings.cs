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
    /// model is obtained from the IShellSettingsManager, which by default reads this
    /// from the App_Data appsettings.json files.
    /// </summary>
    public class ShellSettings : IEquatable<ShellSettings>
    {
        private IConfiguration _configuration { get; set; }
        private MemoryConfigurationProvider _memoryConfigurationProvider =
            new MemoryConfigurationProvider(new MemoryConfigurationSource());

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
        }

        public string Name { get; set; }
        public string RequestUrlHost { get; set; }
        public string RequestUrlPrefix { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TenantState State { get; set; } = TenantState.Invalid;

        [JsonIgnore]
        public IConfigurationBuilder ConfigurationBuilder { get; set; } = new ConfigurationBuilder();

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
                            _configuration = ConfigurationBuilder
                                .AddConfiguration(new ConfigurationRoot(
                                    new[] { _memoryConfigurationProvider }))
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
                lock (this)
                {
                    _memoryConfigurationProvider.Set(key, value);
                }
            }
        }

        private StringValues StringValues => new StringValues(new []
        {
            Name, RequestUrlHost, RequestUrlPrefix, /*DatabaseProvider,
            TablePrefix, ConnectionString, RecipeName, Secret,*/ State.ToString()
        }.Concat(_memoryConfigurationProvider
            .OrderBy(p => p.Key)
            .Select(p => p.Value)).ToArray());

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
