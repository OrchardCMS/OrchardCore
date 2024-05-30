using System;
using OrchardCore.Testing.Apis.Security;

namespace OrchardCore.Testing.Apis
{
    public static class SiteContextExtensions
    {
        public static ISiteContext WithDatabaseProvider(this ISiteContext siteContext, string databaseProvider)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(databaseProvider, nameof(databaseProvider));

            siteContext.Options.DatabaseProvider = databaseProvider;

            return siteContext;
        }

        public static ISiteContext WithConnectionString(this ISiteContext siteContext, string connectionString)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

            siteContext.Options.ConnectionString = connectionString;

            return siteContext;
        }

        public static ISiteContext WithPermissionsContext(this ISiteContext siteContext, PermissionsContext permissionsContext)
        {
            ArgumentNullException.ThrowIfNull(permissionsContext, nameof(permissionsContext));

            siteContext.Options.PermissionsContext = permissionsContext;

            return siteContext;
        }

        public static ISiteContext WithRecipe(this ISiteContext siteContext, string recipeName)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(recipeName, nameof(recipeName));

            siteContext.Options.RecipeName = recipeName;

            return siteContext;
        }
    }
}
