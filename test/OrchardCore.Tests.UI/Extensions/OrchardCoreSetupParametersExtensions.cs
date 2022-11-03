using Lombiq.Tests.UI.Services;
using OrchardCore.Tests.UI.Helpers;

namespace Lombiq.Tests.UI.Pages
{
    public static class OrchardCoreSetupParametersExtensions
    {
        public static OrchardCoreSetupParameters ConfigureDatabaseSettings(
            this OrchardCoreSetupParameters setupParameters, UITestContext context)
        {
            if (DatabaseProviderHelper.GetCIDatabaseProvider() == OrchardCoreSetupPage.DatabaseType.Postgres)
            {
                setupParameters.DatabaseProvider = OrchardCoreSetupPage.DatabaseType.Postgres;
                setupParameters.ConnectionString = "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=postgres;";
                setupParameters.TablePrefix = context.Id.Replace('-', '_');
            }

            return setupParameters;
        }
    }
}
