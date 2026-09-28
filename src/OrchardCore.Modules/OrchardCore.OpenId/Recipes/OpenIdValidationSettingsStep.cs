using System.Text.Json.Nodes;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes;

/// <summary>
/// This recipe step sets Token Validation OpenID Connect settings.
/// </summary>
public sealed class OpenIdValidationSettingsStep : NamedRecipeStepHandler
{
    private readonly IOpenIdValidationService _validationService;

    public OpenIdValidationSettingsStep(IOpenIdValidationService validationService)
        : base(nameof(OpenIdValidationSettings))
    {
        _validationService = validationService;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<OpenIdValidationSettingsStepModel>();
        var settings = await _validationService.LoadSettingsAsync();

        settings.Tenant = model.OpenIdValidationSettings.Tenant;
        settings.MetadataAddress = model.OpenIdValidationSettings.MetadataAddress;
        settings.Authority = model.OpenIdValidationSettings.Authority;
        settings.Audience = model.OpenIdValidationSettings.Audience;
        settings.DisableTokenTypeValidation = model.OpenIdValidationSettings.DisableTokenTypeValidation;

        await _validationService.UpdateSettingsAsync(settings);
    }
}
