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
        private ShellConfiguration _settings;
        private ShellConfiguration _configuration;

        private string _requestUrlHost;
        private string _requestUrlPrefix;
        private TenantState _state;

        public ShellSettings()
        {
            _settings = new ShellConfiguration();
            _configuration = new ShellConfiguration();

            InitLocals();
        }

        public ShellSettings(ShellConfiguration settings, ShellConfiguration configuration)
        {
            _settings = settings;
            _configuration = configuration;

            InitLocals();
        }

        public ShellSettings(ShellSettings settings)
        {
            _settings = new ShellConfiguration(settings._settings);
            _configuration = new ShellConfiguration(settings.Name, settings._configuration);

            Name = settings.Name;

            InitLocals();
        }

        private void InitLocals()
        {
            _state = _settings.GetValue<TenantState>("State");
            _requestUrlHost = _settings["RequestUrlHost"];
            _requestUrlPrefix = _settings["RequestUrlPrefix"]?.Trim(' ', '/');
        }

        public string Name { get; set; }

        public string RequestUrlHost
        {
            get => _requestUrlHost;
            set => _settings["RequestUrlHost"] = _requestUrlHost = value;
        }

        public string RequestUrlPrefix
        {
            get => _requestUrlPrefix;
            set => _settings["RequestUrlPrefix"] = _requestUrlPrefix = value?.Trim(' ', '/');
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public TenantState State
        {
            get => _state;
            set => _settings["State"] = (_state = value).ToString();
        }

        [JsonIgnore]
        public IShellConfiguration ShellConfiguration => _configuration;

        [JsonIgnore]
        public string this[string key]
        {
            get => _configuration[key];
            set => _configuration[key] = value;
        }
    }
}
