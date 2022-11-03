using Lombiq.Tests.UI.Services;
using OrchardCore.Tests.UI.Helpers;

namespace Lombiq.Tests.UI.Pages
{
    public static class OrchardCoreSetupParametersExtensions
    {
        public static OrchardCoreSetupParameters DatabaseProviderFromEnvironmentIfAvailable(
            this OrchardCoreSetupParameters setupParameters, UITestContext context)
        {
            if (DatabaseProviderHelper.IsDatabaseProviderProvidedByEnvironment())
            {
                setupParameters.DatabaseProvider = OrchardCoreSetupPage.DatabaseType.ProvidedByEnvironment;
            }

            return setupParameters;
        }
    }
}
