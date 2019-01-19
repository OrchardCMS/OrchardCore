using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// Represents the minimalistic set of fields stored for each tenant. This model
    /// is obtained from the 'IShellSettingsManager', which by default reads this
    /// from the 'App_Data/tenants.json' file.
    /// </summary>
    public class ShellSettings
    {
        private TenantSettings _settings;
        private TenantConfiguration _configuration;

        public ShellSettings()
        {
            _settings = new TenantSettings();
            _configuration = new TenantConfiguration();
        }

        public ShellSettings(TenantSettings settings, TenantConfiguration configuration)
        {
            _settings = settings;
            _configuration = configuration;
        }

        public ShellSettings(ShellSettings settings)
        {
            _settings = new TenantSettings(settings._settings);
            _configuration = new TenantConfiguration(settings.Name, settings._configuration);

            Name = settings.Name;
        }

        public string Name { get; set; }

        public string RequestUrlHost
        {
            get => _settings["RequestUrlHost"];
            set => _settings["RequestUrlHost"] = value;
        }

        public string RequestUrlPrefix
        {
            get => _settings["RequestUrlPrefix"];
            set => _settings["RequestUrlPrefix"] = value;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public TenantState State
        {
            get => _settings.GetValue<TenantState>("State");
            set => _settings["State"] = value.ToString();
        }

        [JsonIgnore]
        public ITenantConfiguration TenantConfiguration => _configuration;

        [JsonIgnore]
        public string this[string key]
        {
            get => _configuration[key];
            set => _configuration[key] = value;
        }
    }
}
