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

    protected override async Task ProcessAsync(OpenIdValidationDeploymentStep step, DeploymentPlanResult result)
    {
        var validationSettings = await _openIdValidationService.GetSettingsAsync();

        result.Steps.Add(new JsonObject
        {
            ["name"] = nameof(OpenIdValidationSettings),
            ["OpenIdValidationSettings"] = JObject.FromObject(validationSettings),
        });
    }
}
