using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services
{
    public class RecipeEnvironmentFeatureProvider : IRecipeEnvironmentProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RecipeEnvironmentFeatureProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Set later to override any pre existing values.
        public int Order => 1000;

        public Task PopulateEnvironmentAsync(IDictionary<string, object> environment)
        {
            // When a migration is executed during setup these properties are available on the feature.
            var feature = _httpContextAccessor.HttpContext.Features.Get<RecipeEnvironmentFeature>();
            if (feature != null)
            {
                if (feature.Properties.TryGetValue("AdminUserId", out var adminUserId))
                {
                    environment["AdminUserId"] = adminUserId;
                }
                if (feature.Properties.TryGetValue("AdminUsername", out var adminUserName))
                {
                    environment["AdminUsername"] = adminUserName;
                }
                if (feature.Properties.TryGetValue("SiteName", out var siteName))
                {
                    environment["SiteName"] = siteName;
                }
            }

            return Task.CompletedTask;
        }
    }
}
