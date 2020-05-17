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
    /// This recipe step sets general reset password settings.
    /// </summary>
    public class ResetPasswordSettingsStep : IRecipeStepHandler
    {
        private readonly ISiteService _siteService;

        public ResetPasswordSettingsStep(ISiteService siteService)
            => _siteService = siteService;

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(ResetPasswordSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<ResetPasswordSettingsStepModel>();

            var site = await _siteService.LoadSiteSettingsAsync();
            var settings = site.As<ResetPasswordSettings>();
            settings.AllowResetPassword = model.AllowResetPassword;
            settings.UseSiteTheme = model.UseSiteTheme;
            site.Put<ResetPasswordSettings>(settings);
            await _siteService.UpdateSiteSettingsAsync(site);
        }
    }
}
