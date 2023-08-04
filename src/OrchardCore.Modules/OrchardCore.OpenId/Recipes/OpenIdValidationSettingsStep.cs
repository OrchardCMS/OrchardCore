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
            if (!String.Equals(context.Name, nameof(OpenIdValidationSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<OpenIdValidationSettingsStepModel>();
            var settings = await _validationService.LoadSettingsAsync();

            settings.Tenant = model.Tenant;
            settings.MetadataAddress = !String.IsNullOrEmpty(model.MetadataAddress) ? new Uri(model.MetadataAddress, UriKind.Absolute) : null;
            settings.Authority = !String.IsNullOrEmpty(model.Authority) ? new Uri(model.Authority, UriKind.Absolute) : null;
            settings.Audience = model.Audience;
            settings.DisableTokenTypeValidation = model.DisableTokenTypeValidation;

            await _validationService.UpdateSettingsAsync(settings);
        }
    }
}
