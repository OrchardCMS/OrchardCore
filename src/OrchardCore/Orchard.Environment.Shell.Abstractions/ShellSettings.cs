using System;
using System.Collections.Generic;
using Orchard.Environment.Shell.Models;

namespace Orchard.Environment.Shell
{
    /// <summary>
    /// Represents the minimalistic set of fields stored for each tenant. This
    /// model is obtained from the IShellSettingsManager, which by default reads this
    /// from the App_Data settings.txt files.
    /// </summary>
    public class ShellSettings
    {
        public IDictionary<string, string> Configuration { get; private set; }

        public ShellSettings() : this(new Dictionary<string, string>()) { }

        public ShellSettings(IDictionary<string, string> configuration)
        {
            Configuration = configuration;

            if (!configuration.ContainsKey("State") || !Enum.TryParse(configuration["State"], true, out _state)) {
                _state = TenantState.Invalid;
            }
        }

        public string Name
        {
            get => GetValue("Name");
            set => Configuration["Name"] = value;
        }

        public string RequestUrlHost
        {
            get => GetValue("RequestUrlHost");
            set => Configuration["RequestUrlHost"] = value;
        }

        public string RequestUrlPrefix
        {
            get => GetValue("RequestUrlPrefix");
            set => Configuration["RequestUrlPrefix"] = value;
        }

        public string DatabaseProvider
        {
            get => GetValue("DatabaseProvider");
            set => Configuration["DatabaseProvider"] = value;
        }

        public string TablePrefix
        {
            get => GetValue("TablePrefix");
            set => Configuration["TablePrefix"] = value;
        }

        public string ConnectionString
        {
            get => GetValue("ConnectionString");
            set => Configuration["ConnectionString"] = value;
        }

        private TenantState _state;

        public TenantState State
        {
            get => _state;
            set
            {
                _state = value;
                Configuration["State"] = value.ToString();
            }
        }

        private string GetValue(string key)
        {
            if (!Configuration.ContainsKey(key))
            {
                return null;
            }
            return Configuration[key];
        }
    }
}
