using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Settings
{
    public class RecipeEnvironmentSiteNameProvider : IRecipeEnvironmentProvider
    {
        private readonly ISiteService _siteService;

        public RecipeEnvironmentSiteNameProvider(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task SetEnvironmentAsync(IDictionary<string, object> environment)
        {
            // When these have already been set by another provider, do not reset them.
            if (!environment.ContainsKey(nameof(ISite.SiteName)))
            {
                var siteSettings = await _siteService.GetSiteSettingsAsync();
                if (!String.IsNullOrEmpty(siteSettings.SiteName))
                {
                    environment[nameof(SiteSettings.SiteName)] = siteSettings.SiteName;
                }
            }
        }
    }
}
