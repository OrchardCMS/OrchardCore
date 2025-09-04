using OrchardCore.Recipes.Services;

namespace OrchardCore.Settings;

public class RecipeEnvironmentSiteNameProvider : IRecipeEnvironmentProvider
{
    private readonly ISiteService _siteService;

    public RecipeEnvironmentSiteNameProvider(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public int Order => 0;

    public async Task PopulateEnvironmentAsync(IDictionary<string, object> environment)
    {
        var siteSettings = await _siteService.GetSiteSettingsAsync();
        if (!string.IsNullOrEmpty(siteSettings.SiteName))
        {
            environment[nameof(SiteSettings.SiteName)] = siteSettings.SiteName;
        }
    }
}
