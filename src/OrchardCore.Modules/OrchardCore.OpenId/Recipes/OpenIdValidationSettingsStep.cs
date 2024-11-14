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

        settings.Tenant = model.Tenant;
        settings.MetadataAddress = !string.IsNullOrEmpty(model.MetadataAddress) ? new Uri(model.MetadataAddress, UriKind.Absolute) : null;
        settings.Authority = !string.IsNullOrEmpty(model.Authority) ? new Uri(model.Authority, UriKind.Absolute) : null;
        settings.Audience = model.Audience;
        settings.DisableTokenTypeValidation = model.DisableTokenTypeValidation;

        await _validationService.UpdateSettingsAsync(settings);
    }
}
