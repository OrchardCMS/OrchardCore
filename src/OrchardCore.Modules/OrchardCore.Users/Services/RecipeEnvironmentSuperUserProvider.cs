using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.Users.Services
{
    public class RecipeEnvironmentSuperUserProvider : IRecipeEnvironmentProvider
    {
        private readonly ISiteService _siteService;
        private readonly IServiceProvider _serviceProvider;
        private IUserService _userService;
        private readonly ILogger _logger;

        public RecipeEnvironmentSuperUserProvider(
            ISiteService siteService,
            IServiceProvider serviceProvider,
            ILogger<RecipeEnvironmentSuperUserProvider> logger)
        {
            _siteService = siteService;
            _serviceProvider = serviceProvider;
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
                    // 'IUserService' has many dependencies including options configurations code, particularly with 'OpenId',
                    // so, because this 'IRecipeEnvironmentProvider' may be injected by an `IDataMigration` even if there is no
                    // migration to do, 'IUserService' is lazily resolved, so that it is not injected on each shell activation.
                    _userService ??= _serviceProvider.GetRequiredService<IUserService>();

                    var superUser = await _userService.GetUserByUniqueIdAsync(siteSettings.SuperUser);
                    if (superUser != null)
                    {
                        environment["AdminUserId"] = siteSettings.SuperUser;
                        environment["AdminUsername"] = superUser.UserName;
                    }
                }
                catch
                {
                    _logger.LogWarning("Could not lookup the admin user, user migrations may not have run yet.");
                }
            }
        }
    }
}
