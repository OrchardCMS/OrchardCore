using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// Represents the minimalistic set of fields stored for each tenant. This
    /// model is obtained from the IShellSettingsManager, which by default reads this
    /// from the App_Data appsettings.json files.
    /// </summary>
    public class ShellSettings
    {
        public ShellSettings()
        {
        }

        public ShellSettings(ShellSettings shellSettings)
        {
            Name = shellSettings.Name;
            RequestUrlHost = shellSettings.RequestUrlHost;
            RequestUrlPrefix = shellSettings.RequestUrlPrefix;
            DatabaseProvider = shellSettings.DatabaseProvider;
            TablePrefix = shellSettings.TablePrefix;
            ConnectionString = shellSettings.ConnectionString;
            RecipeName = shellSettings.RecipeName;
            Secret = shellSettings.Secret;
            State = shellSettings.State;
        }

        public string Name { get; set; } = null;
        public string RequestUrlHost { get; set; } = null;
        public string RequestUrlPrefix { get; set; } = null;
        public string DatabaseProvider { get; set; } = null;
        public string TablePrefix { get; set; } = null;
        public string ConnectionString { get; set; } = null;
        public string RecipeName { get; set; } = null;
        public string Secret { get; set; } = null;
        public TenantState State { get; set; } = TenantState.Invalid;
    }
}
