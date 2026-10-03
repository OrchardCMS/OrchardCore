using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Recipes;

public sealed class OpenIdValidationSettingsStepModel
{
    public OpenIdValidationSettings OpenIdValidationSettings { get; set; } = new OpenIdValidationSettings();
}
