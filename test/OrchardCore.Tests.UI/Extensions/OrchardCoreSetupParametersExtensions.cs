using Lombiq.Tests.UI.Services;
using Microsoft.Extensions.Configuration;

namespace Lombiq.Tests.UI.Pages
{
    public static class OrchardCoreSetupParametersExtensions
    {
        public static OrchardCoreSetupParameters DatabaseProviderFromEnvironmentIfAvailable(
            this OrchardCoreSetupParameters setupParameters, UITestContext context)
        {
            var databaseProvider = TestConfigurationManager.RootConfiguration
                .GetSection("OrchardCore")
                .GetValue<string>("DatabaseProvider");

            if (!string.IsNullOrEmpty(databaseProvider) && (databaseProvider is "Postgres" or "MySql"))
            {
                setupParameters.DatabaseProvider = OrchardCoreSetupPage.DatabaseType.ProvidedByEnvironment;
                setupParameters.TablePrefix = context.Id;
            }

            return setupParameters;
        }
    }
}
