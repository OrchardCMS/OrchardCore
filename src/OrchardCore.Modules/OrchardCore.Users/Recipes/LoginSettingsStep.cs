using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Entities;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Recipes
{
    /// <summary>
    /// This recipe step sets general login settings.
    /// </summary>
    public class LoginSettingsStep : IRecipeStepHandler
    {
        private readonly ISiteService _siteService;

        public LoginSettingsStep(ISiteService siteService)
            => _siteService = siteService;

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(LoginSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<LoginSettingsStepModel>();

            var site = await _siteService.LoadSiteSettingsAsync();
            var settings = site.As<LoginSettings>();
            settings.UseSiteTheme = model.UseSiteTheme;
            settings.UseExternalProviderIfOnlyOneDefined = model.UseExternalProviderIfOnlyOneDefined;
            settings.DisableLocalLogin = model.DisableLocalLogin;
            settings.UseScriptToSyncRoles = model.UseScriptToSyncRoles;
            settings.SyncRolesScript = model.SyncRolesScript;
            site.Put<LoginSettings>(settings);
            await _siteService.UpdateSiteSettingsAsync(site);
        }
    }
}
