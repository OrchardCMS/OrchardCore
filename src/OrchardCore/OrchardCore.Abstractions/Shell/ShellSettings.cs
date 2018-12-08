using System;
using System.Collections.Generic;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// Represents the minimalistic set of fields stored for each tenant. This
    /// model is obtained from the IShellSettingsManager, which by default reads this
    /// from the App_Data settings.txt files.
    /// </summary>
    public class ShellSettings
    {
        public ShellSettings()
        {
        }

        public ShellSettings(ShellSettings shellSettings)
        {
            Name = shellSettings.Name;
            ConnectionString = shellSettings.ConnectionString;
            DatabaseProvider = shellSettings.DatabaseProvider;
            RecipeName = shellSettings.RecipeName;
            RequestUrlHost = shellSettings.RequestUrlHost;
            RequestUrlPrefix = shellSettings.RequestUrlPrefix;
            Secret = shellSettings.Secret;
            State = shellSettings.State;
            TablePrefix = shellSettings.TablePrefix;
        }

        public string Name { get; set; }
        public string RequestUrlHost { get; set; }
        public string RequestUrlPrefix { get; set; }
        public string DatabaseProvider { get; set; }
        public string TablePrefix { get; set; }
        public string ConnectionString { get; set; }
        public string RecipeName { get; set; }
        public string Secret { get; set; }
        public TenantState State { get; set; }
    }
}
