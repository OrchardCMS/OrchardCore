using System;
using OrchardCore.Testing.Apis.Security;

namespace OrchardCore.Testing.Apis
{
    public static class SiteContextExtensions
    {
        public static ISiteContext WithDatabaseProvider(this ISiteContext siteContext, string databaseProvider)
        {
            if (String.IsNullOrEmpty(databaseProvider))
            {
                throw new ArgumentException($"'{nameof(databaseProvider)}' cannot be null or empty.", nameof(databaseProvider));
            }

            siteContext.Options.DatabaseProvider = databaseProvider;

            return siteContext;
        }

        public static ISiteContext WithConnectionString(this ISiteContext siteContext, string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or empty.", nameof(connectionString));
            }

            siteContext.Options.ConnectionString = connectionString;

            return siteContext;
        }

        public static ISiteContext WithPermissionsContext(this ISiteContext siteContext, PermissionsContext permissionsContext)
        {
            if (permissionsContext is null)
            {
                throw new ArgumentNullException(nameof(permissionsContext));
            }

            siteContext.Options.PermissionsContext = permissionsContext;

            return siteContext;
        }

        public static ISiteContext WithRecipe(this ISiteContext siteContext, string recipeName)
        {
            if (String.IsNullOrEmpty(recipeName))
            {
                throw new ArgumentException($"'{nameof(recipeName)}' cannot be null or empty.", nameof(recipeName));
            }

            siteContext.Options.RecipeName = recipeName;

            return siteContext;
        }
    }
}
