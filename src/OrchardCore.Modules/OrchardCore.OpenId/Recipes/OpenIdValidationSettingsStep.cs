using System;
using System.Threading.Tasks;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes
{
    /// <summary>
    /// This recipe step sets Token Validation OpenID Connect settings.
    /// </summary>
    public class OpenIdValidationSettingsStep : IRecipeStepHandler
    {
        private readonly IOpenIdValidationService _validationService;

        public OpenIdValidationSettingsStep(IOpenIdValidationService validationService)
            => _validationService = validationService;

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(OpenIdValidationSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<OpenIdValidationSettingsStepModel>();
            var settings = await _validationService.GetSettingsAsync();
            settings.Tenant = model.Tenant;
            settings.Authority = !string.IsNullOrEmpty(model.Authority) ? new Uri(model.Authority, UriKind.Absolute) : null;
            settings.Audience = model.Audience;

            await _validationService.UpdateSettingsAsync(settings);
        }
    }
}