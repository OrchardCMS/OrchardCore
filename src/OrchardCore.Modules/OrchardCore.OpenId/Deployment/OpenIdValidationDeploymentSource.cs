using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Deployment;

public class OpenIdValidationDeploymentSource
    : DeploymentSourceBase<OpenIdValidationDeploymentStep>
{
    private readonly IOpenIdValidationService _openIdValidationService;

    public OpenIdValidationDeploymentSource(IOpenIdValidationService openIdValidationService)
    {
        _openIdValidationService = openIdValidationService;
    }

    public override async Task ProcessDeploymentStepAsync(DeploymentPlanResult result)
    {
        var validationSettings = await _openIdValidationService.GetSettingsAsync();

        // The 'name' property should match the related recipe step name.
        var jObject = new JsonObject { ["name"] = nameof(OpenIdValidationSettings) };

        // Merge settings as the recipe step doesn't use a child property.
        jObject.Merge(JObject.FromObject(validationSettings));

        result.Steps.Add(jObject);
    }
}
