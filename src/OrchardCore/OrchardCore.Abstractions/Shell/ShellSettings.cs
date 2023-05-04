using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrchardCore.Clusters;
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
        public const int ClusterSlotCount = 16383;
        private static readonly char[] _hostSeparators = new[] { ',', ' ' };

        private readonly ShellConfiguration _settings;
        private readonly ShellConfiguration _configuration;
        private volatile int _clusterSlot = -1;

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
            set
            {
                _settings["TenantId"] ??= _settings["VersionId"] ?? value;
                _settings["VersionId"] = value;
            }
        }

        public string TenantId => _settings["TenantId"] ?? _settings["VersionId"];

        [JsonIgnore]
        public int ClusterSlot
        {
            get
            {
                if (_clusterSlot == -1)
                {
                    var tenantId = TenantId;
                    if (tenantId is not null)
                    {
                        Interlocked.Exchange(ref _clusterSlot, Crc16XModem.Compute(tenantId) % ClusterSlotCount);
                    }
                }

                return _clusterSlot;
            }
        }

        public string RequestUrlHost
        {
            get => _settings["RequestUrlHost"];
            set => _settings["RequestUrlHost"] = value;
        }

        [JsonIgnore]
        public string[] RequestUrlHosts => _settings["RequestUrlHost"]
            ?.Split(_hostSeparators, StringSplitOptions.RemoveEmptyEntries)
            ?? Array.Empty<string>();

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
