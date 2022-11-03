using Lombiq.Tests.UI.Services;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Tests.UI.Helpers
{
    internal static class DatabaseProviderHelper
    {
        public static bool IsDatabaseProviderProvidedByEnvironment()
        {
            var databaseProvider = TestConfigurationManager.RootConfiguration
                .GetSection("OrchardCore")
                .GetValue<string>("DatabaseProvider");

            return !string.IsNullOrEmpty(databaseProvider) && (databaseProvider is "Postgres" or "MySql");
        }
    }
}
