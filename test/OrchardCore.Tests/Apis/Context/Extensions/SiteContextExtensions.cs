using OrchardCore.Testing.Apis.Security;

namespace OrchardCore.Tests.Apis.Context;
public static class SiteContextExtensions
{
    public static T WithDatabaseProvider<T>(this T siteContext, string databaseProvider) where T : SiteContext
    {
        siteContext.DatabaseProvider = databaseProvider;
        return siteContext;
    }

    public static T WithConnectionString<T>(this T siteContext, string connectionString) where T : SiteContext
    {
        siteContext.ConnectionString = connectionString;
        return siteContext;
    }

    public static T WithPermissionsContext<T>(this T siteContext, PermissionsContext permissionsContext) where T : SiteContext
    {
        siteContext.PermissionsContext = permissionsContext;
        return siteContext;
    }

    public static T WithRecipe<T>(this T siteContext, string recipeName) where T : SiteContext
    {
        siteContext.RecipeName = recipeName;
        return siteContext;
    }
}
