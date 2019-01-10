using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrchardCore.Abstractions.Shell;
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
        private ShellConfiguration _configuration;
        private string _requestUrlHost;
        private string _requestUrlPrefix;

        public ShellSettings()
        {
            _configuration = new ShellConfiguration();
        }

        public ShellSettings(ShellConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ShellSettings(ShellSettings settings)
        {
            _configuration = new ShellConfiguration(settings.Name, settings._configuration);

            Name = settings.Name;
            RequestUrlHost = settings.RequestUrlHost;
            RequestUrlPrefix = settings.RequestUrlPrefix;
            State = settings.State;
        }

        public string Name { get; set; }

        public string RequestUrlHost
        {
            get => _requestUrlHost;
            set => _requestUrlHost = value ?? _requestUrlHost;
        }

        public string RequestUrlPrefix
        {
            get => _requestUrlPrefix;
            set => _requestUrlPrefix = value ?? _requestUrlPrefix;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public TenantState State { get; set; } = TenantState.Invalid;

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
