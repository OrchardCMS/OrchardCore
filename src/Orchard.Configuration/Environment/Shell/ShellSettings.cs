using Microsoft.Framework.Configuration;
using System;
using System.Collections.Generic;

namespace Orchard.Configuration.Environment {
    /// <summary>
    /// Represents the minimalistic set of fields stored for each tenant. This 
    /// model is obtained from the IShellSettingsManager, which by default reads this
    /// from the App_Data settings.txt files.
    /// </summary>
    public class ShellSettings {
        private TenantState _tenantState;
        public ShellSettings() {
            RootConfiguration = new ConfigurationRoot(new[] { new InternalConfigurationSource() });
            State = TenantState.Invalid;
        }

        public ShellSettings(ShellSettings settings) {
            RootConfiguration = new ConfigurationRoot(new[] { new InternalConfigurationSource() });

            Name = settings.Name;
            //DataProvider = settings.DataProvider;
            //DataConnectionString = settings.DataConnectionString;
            //DataTablePrefix = settings.DataTablePrefix;
            RequestUrlHost = settings.RequestUrlHost;
            RequestUrlPrefix = settings.RequestUrlPrefix;
            State = settings.State;
        }

        public ShellSettings(IConfigurationRoot configuration) {
            RootConfiguration = configuration;

            TenantState state;
            State = Enum.TryParse(configuration["State"], true, out state)
                ? state
                : TenantState.Uninitialized;
        }
             

        /// <summary>
        /// Gets all keys held by this shell settings.
        /// </summary>
        public IConfigurationRoot RootConfiguration { get; private set; }

        /// <summary>
        /// The name of the tenant
        /// </summary>
        public string Name
        {
            get { return RootConfiguration["Name"]; }
            set { RootConfiguration["Name"] = value; }
        }

        /// <summary>
        /// The database provider for the tenant
        /// </summary>
        //public DataProvider[] DataProvider {
        //    get { return RootConfiguration.GetSection("DataProviders"); }
        //    set { this["DataProvider"] = value; }
        //}

        ///// <summary>
        ///// The database connection string
        ///// </summary>
        //public string DataConnectionString
        //{
        //    get { return this["DataConnectionString"]; }
        //    set { this["DataConnectionString"] = value; }
        //}

        ///// <summary>
        ///// The data table prefix added to table names for this tenant
        ///// </summary>
        //public string DataTablePrefix
        //{
        //    get { return _configuration["DataTablePrefix"]; }
        //    set { _configuration["DataTablePrefix"] = value; }
        //}

        /// <summary>
        /// The host name of the tenant
        /// </summary>
        public string RequestUrlHost
        {
            get { return RootConfiguration["RequestUrlHost"]; }
            set { RootConfiguration["RequestUrlHost"] = value; }
        }

        /// <summary>
        /// The request url prefix of the tenant
        /// </summary>
        public string RequestUrlPrefix
        {
            get { return RootConfiguration["RequestUrlPrefix"]; }
            set { RootConfiguration["RequestUrlPrefix"] = value; }
        }

        /// <summary>
        /// The state is which the tenant is
        /// </summary>
        public TenantState State {
            get { return _tenantState; }
            set {
                _tenantState = value;
                RootConfiguration["State"] = value.ToString();
            }
        }
    }
}