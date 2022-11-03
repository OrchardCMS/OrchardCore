using System;
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

            // Since apart from SQLite and SQL Server all the tests will use the same, single DB, each test and each of
            // the executions (if repeated due to transient errors) should use a unique table prefix.
            // Table names in PostgreSQL should begin with a letter mustn't contain hyphens
            // (https://www.postgresql.org/docs/7.0/syntax525.htm), and can be at most 63 characters long. In
            // MySQL/MariaDB the rules are similar (table names can start with numbers but it's safer if they don't,
            // and the max table name length is 64 characters), see https://mariadb.com/kb/en/identifier-names/. With
            // the leading "t" and the no-hyphen context ID it would be 34 characters, so we have to shorten it. With
            // the hash it will be at most 11 characters (since negative hash codes are disallowed).
            setupParameters.TablePrefix = "t" + Math.Abs(context.Id.Replace("-", String.Empty).GetHashCode());

            if (provider == OrchardCoreSetupPage.DatabaseType.Postgres)
            {
                setupParameters.ConnectionString = "Host=localhost;Port=5432;User ID=postgres;Password=postgres;Database=postgres;";
            }
            else if (provider == OrchardCoreSetupPage.DatabaseType.MySql)
            {
                setupParameters.ConnectionString = "server=127.0.0.1;uid=root;pwd=test123;database=mariadb;";
            }

            return setupParameters;
        }
    }
}
