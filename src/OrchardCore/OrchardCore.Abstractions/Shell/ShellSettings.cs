using System.Threading.Tasks;
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
        private readonly ShellConfiguration _settings;
        private readonly ShellConfiguration _configuration;

        public ShellSettings()
        {
            _settings = new ShellConfiguration();
            _configuration = new ShellConfiguration();
        }

        public ShellSettings(ShellConfiguration settings, ShellConfiguration configuration)
        {
            _settings = settings;
            _configuration = configuration;
        }

        public ShellSettings(ShellSettings settings)
        {
            _settings = new ShellConfiguration(settings._settings);
            _configuration = new ShellConfiguration(settings.Name, settings._configuration);
            Name = settings.Name;
        }

        public string Name { get; set; }

        public string VersionId
        {
            get => _settings["VersionId"];
            set => _settings["VersionId"] = value;
        }

        public string RequestUrlHost
        {
            get => _settings["RequestUrlHost"];
            set => _settings["RequestUrlHost"] = value;
        }

        public string RequestUrlPrefix
        {
            get => _settings["RequestUrlPrefix"]?.Trim(' ', '/');
            set => _settings["RequestUrlPrefix"] = value;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public TenantState State
        {
            get => _settings.GetValue<TenantState>("State");
            set => _settings["State"] = value.ToString();
        }

        [JsonIgnore]
        public IShellConfiguration ShellConfiguration => _configuration;

        [JsonIgnore]
        public string this[string key]
        {
            get => _configuration[key];
            set => _configuration[key] = value;
        }

        public Task EnsureConfigurationAsync() => _configuration.EnsureConfigurationAsync();

        public bool IsDefaultShell() => Name == ShellHelper.DefaultShellName;
    }
}
