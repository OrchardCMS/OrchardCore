using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
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
    public class ShellSettings : IEquatable<ShellSettings>
    {
        private ShellConfiguration _configuration;

        public ShellSettings(IConfigurationBuilder builder = null)
        {
            _configuration = new ShellConfiguration(builder);
        }

        public ShellSettings(ShellSettings settings)
       {
            _configuration = settings._configuration;

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
            get => Configuration[key];
            set => Configuration[key] = value;
        }

        internal StringValues StringValues => new StringValues(new[]
            { Name, RequestUrlHost, RequestUrlPrefix, State.ToString() });

        public bool Equals(ShellSettings other)
        {
            return StringValues.Equals(other?.StringValues ?? String.Empty) &&
                (_configuration.Equals(other?._configuration));
        }

        public override int GetHashCode()
        {
            return StringValues.GetHashCode();
        }
    }
}
