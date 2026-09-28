using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Deployment;

public sealed class OpenIdValidationDeploymentSource
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

        var json = JObject.FromObject(validationSettings);
        json.TryAdd("name", nameof(OpenIdValidationSettings));

        result.Steps.Add(json);
    }
}
