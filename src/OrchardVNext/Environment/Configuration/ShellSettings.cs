using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardVNext.Environment.Configuration {
    /// <summary>
    /// Represents the minimalistic set of fields stored for each tenant. This 
    /// model is obtained from the IShellSettingsManager, which by default reads this
    /// from the App_Data settings.txt files.
    /// </summary>
    public class ShellSettings {
        public const string DefaultName = "Default";
        private TenantState _tenantState;
        private readonly IDictionary<string, string> _values;

        public ShellSettings() {
            _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            State = TenantState.Invalid;
        }

        public ShellSettings(ShellSettings settings) {
            _values = new Dictionary<string, string>(settings._values, StringComparer.OrdinalIgnoreCase);

            Name = settings.Name;
            DataProvider = settings.DataProvider;
            DataConnectionString = settings.DataConnectionString;
            DataTablePrefix = settings.DataTablePrefix;
            RequestUrlHost = settings.RequestUrlHost;
            RequestUrlPrefix = settings.RequestUrlPrefix;
            State = settings.State;
        }

        public string this[string key] {
            get {
                string retVal;
                return _values.TryGetValue(key, out retVal) ? retVal : null;
            }
            set { _values[key] = value; }
        }

        /// <summary>
        /// Gets all keys held by this shell settings.
        /// </summary>
        public IEnumerable<string> Keys => _values.Keys;

        /// <summary>
        /// The name of the tenant
        /// </summary>
        public string Name
        {
            get { return this["Name"] ?? ""; }
            set { this["Name"] = value; }
        }

        /// <summary>
        /// The database provider for the tenant
        /// </summary>
        public string DataProvider
        {
            get { return this["DataProvider"] ?? ""; }
            set { this["DataProvider"] = value; }
        }

        /// <summary>
        /// The database connection string
        /// </summary>
        public string DataConnectionString
        {
            get { return this["DataConnectionString"]; }
            set { this["DataConnectionString"] = value; }
        }

        /// <summary>
        /// The data table prefix added to table names for this tenant
        /// </summary>
        public string DataTablePrefix
        {
            get { return this["DataTablePrefix"]; }
            set { this["DataTablePrefix"] = value; }
        }

        /// <summary>
        /// The host name of the tenant
        /// </summary>
        public string RequestUrlHost
        {
            get { return this["RequestUrlHost"]; }
            set { this["RequestUrlHost"] = value; }
        }

        /// <summary>
        /// The request url prefix of the tenant
        /// </summary>
        public string RequestUrlPrefix
        {
            get { return this["RequestUrlPrefix"]; }
            set { _values["RequestUrlPrefix"] = value; }
        }

        /// <summary>
        /// The state is which the tenant is
        /// </summary>
        public TenantState State {
            get { return _tenantState; }
            set {
                _tenantState = value;
                this["State"] = value.ToString();
            }
        }
    }
}