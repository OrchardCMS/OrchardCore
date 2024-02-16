using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public OpenIdValidationSettingsStep(
            IOpenIdValidationService validationService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _validationService = validationService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(OpenIdValidationSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<OpenIdValidationSettingsStepModel>(_jsonSerializerOptions);
            var settings = await _validationService.LoadSettingsAsync();

            settings.Tenant = model.Tenant;
            settings.MetadataAddress = !string.IsNullOrEmpty(model.MetadataAddress) ? new Uri(model.MetadataAddress, UriKind.Absolute) : null;
            settings.Authority = !string.IsNullOrEmpty(model.Authority) ? new Uri(model.Authority, UriKind.Absolute) : null;
            settings.Audience = model.Audience;
            settings.DisableTokenTypeValidation = model.DisableTokenTypeValidation;

            await _validationService.UpdateSettingsAsync(settings);
        }
    }
}
