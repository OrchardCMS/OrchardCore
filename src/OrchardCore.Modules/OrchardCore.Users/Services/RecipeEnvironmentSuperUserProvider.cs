using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.Users.Services
{
    public class RecipeEnvironmentSuperUserProvider : IRecipeEnvironmentProvider
    {
        private readonly ISiteService _siteService;
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public RecipeEnvironmentSuperUserProvider(
            ISiteService siteService,
            IUserService userService,
            ILogger<RecipeEnvironmentSuperUserProvider> logger)
        {
            _siteService = siteService;
            _userService = userService;
            _logger = logger;
        }

        public int Order => 0;

        public async Task PopulateEnvironmentAsync(IDictionary<string, object> environment)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            if (!String.IsNullOrEmpty(siteSettings.SuperUser))
            {
                try
                {
                    var superUser = await _userService.GetUserByUniqueIdAsync(siteSettings.SuperUser);
                    if (superUser != null)
                    {
                        environment["AdminUserId"] = siteSettings.SuperUser;
                    }
                } 
                catch 
                {
                    _logger.LogWarning("Could not lookup AdminUserId; User migrations may not have run yet");
                }
            }
        }
    }
}
