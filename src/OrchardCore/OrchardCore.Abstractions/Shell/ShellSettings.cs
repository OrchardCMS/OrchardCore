using System;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// Represents the minimalistic set of fields stored for each tenant. This model
    /// is obtained from the 'IShellSettingsManager', which by default reads this
    /// from the 'App_Data/Sites/tenants.json' file.
    /// </summary>
    public class ShellSettings
    {
        private ShellConfiguration _configuration;

        public ShellSettings() : this(null, null) { }

        public ShellSettings(string name) : this(name, null) { }

        public ShellSettings(string name, Func<string, IConfigurationBuilder> factory)
        {
            _configuration = new ShellConfiguration(name, factory);

            Name = name;
        }

        public ShellSettings(ShellSettings settings)
        {
            _configuration = new ShellConfiguration(settings._configuration);

            Name = settings.Name;
            RequestUrlHost = settings.RequestUrlHost;
            RequestUrlPrefix = settings.RequestUrlPrefix;
            State = settings.State;
        }

        public string Name { get; set; }
        public string RequestUrlHost { get; set; }
        public string RequestUrlPrefix { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TenantState State { get; set; } = TenantState.Invalid;

        /// <summary>
        /// The tenant lazyly built <see cref="IConfiguration"/>.
        /// </summary>
        [JsonIgnore]
        public IConfiguration Configuration => _configuration.Configuration;

        [JsonIgnore]
        public string this[string key]
        {
            get => _configuration[key];
            set => _configuration[key] = value;
        }
    }
}
