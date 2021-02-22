using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.Users.Services
{
    public class RecipeEnvironmentSuperUserProvider : IRecipeEnvironmentProvider
    {
        private readonly ISiteService _siteService;
        private readonly IUserService _userService;

        public RecipeEnvironmentSuperUserProvider(ISiteService siteService, IUserService userService)
        {
            _siteService = siteService;
            _userService = userService;
        }

        public int Order => 0;

        public async Task PopulateEnvironmentAsync(IDictionary<string, object> environment)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            if (!String.IsNullOrEmpty(siteSettings.SuperUser))
            {
                var superUser = await _userService.GetUserByUniqueIdAsync(siteSettings.SuperUser);
                if (superUser != null)
                {
                    environment["AdminUserId"] = siteSettings.SuperUser;
                }
            }
        }
    }
}
