using System;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// Represents the minimalistic set of fields stored for each tenant. This
    /// model is obtained from the IShellSettingsManager, which by default reads this
    /// from the App_Data appsettings.json files.
    /// </summary>
    public class ShellSettings : IEquatable<ShellSettings>
    {
        private TenantState _tenantState;

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

        public string State
        {
            get => _tenantState.ToString();

            set
            {
                if (!Enum.TryParse(value, true, out _tenantState))
                {
                    _tenantState = TenantState.Invalid;
                }
            }
        }

        public TenantState GetState() => _tenantState;

        public void SetState(TenantState state)
        {
            _tenantState = state;
            State = state.ToString();
        }

        private StringValues StringValues => new StringValues(new []
        {
            Name, RequestUrlHost, RequestUrlPrefix, DatabaseProvider,
            TablePrefix, ConnectionString, RecipeName, Secret, State
        });

        public bool Equals(ShellSettings other)
        {
            return StringValues.Equals(other?.StringValues ?? String.Empty);
        }

        public override int GetHashCode()
        {
            return StringValues.GetHashCode();
        }
    }
}
