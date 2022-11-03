using Lombiq.Tests.UI.Services;
using OrchardCore.Tests.UI.Helpers;

namespace Lombiq.Tests.UI.Pages
{
    public static class OrchardCoreSetupParametersExtensions
    {
        public static OrchardCoreSetupParameters ConfigureDatabaseSettings(
            this OrchardCoreSetupParameters setupParameters, UITestContext context)
        {
            var provider = DatabaseProviderHelper.GetCIDatabaseProvider();

            if (provider is not (OrchardCoreSetupPage.DatabaseType.Postgres or OrchardCoreSetupPage.DatabaseType.MySql))
            {
                return setupParameters;
            }

            setupParameters.DatabaseProvider = provider;

            // Table names in PostgreSQL should begin with a letter and mustn't contain hyphens
            // (https://www.postgresql.org/docs/7.0/syntax525.htm). In MariaDB the rules are similar (table names can
            // start with numbers but it's safer if they don't), see https://mariadb.com/kb/en/identifier-names/.
            setupParameters.TablePrefix = "test" + context.Id.Replace("-", "");

            if (provider == OrchardCoreSetupPage.DatabaseType.Postgres)
            {
                setupParameters.ConnectionString = "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=postgres;";

            }
            else if (provider == OrchardCoreSetupPage.DatabaseType.MySql)
            {
                setupParameters.ConnectionString = "server=mariadb;uid=root;pwd=test123;database=test";
            }

            return setupParameters;
        }
    }
}
