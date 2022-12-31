using System;
using OrchardCore.Testing.Apis.Security;

namespace OrchardCore.Tests.Apis.Context
{
    public static class SiteContextExtensions
    {
        public static SiteContext WithDatabaseProvider(this SiteContext siteContext, string databaseProvider)
        {
            if (String.IsNullOrEmpty(databaseProvider))
            {
                throw new ArgumentException($"'{nameof(databaseProvider)}' cannot be null or empty.", nameof(databaseProvider));
            }

            siteContext.DatabaseProvider = databaseProvider;

            return siteContext;
        }

        public static SiteContext WithConnectionString(this SiteContext siteContext, string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or empty.", nameof(connectionString));
            }

            siteContext.ConnectionString = connectionString;
            return siteContext;
        }

        public static SiteContext WithPermissionsContext(this SiteContext siteContext, PermissionsContext permissionsContext)
        {
            if (permissionsContext is null)
            {
                throw new ArgumentNullException(nameof(permissionsContext));
            }

            siteContext.PermissionsContext = permissionsContext;

            return siteContext;
        }

        public static SiteContext WithRecipe(this SiteContext siteContext, string recipeName)
        {
            if (String.IsNullOrEmpty(recipeName))
            {
                throw new ArgumentException($"'{nameof(recipeName)}' cannot be null or empty.", nameof(recipeName));
            }

            siteContext.RecipeName = recipeName;

            return siteContext;
        }
    }
}
