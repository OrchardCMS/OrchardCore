using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Deployment;

public class AzureADDeploymentSource
    : DeploymentSourceBase<AzureADDeploymentStep>
{
    private readonly IAzureADService _azureADService;

    public AzureADDeploymentSource(IAzureADService azureADService)
    {
        _azureADService = azureADService;
    }

    protected override async Task ProcessAsync(AzureADDeploymentStep step, DeploymentPlanResult result)
    {
        var azureADSettings = await _azureADService.GetSettingsAsync();

        result.Steps.Add(new JsonObject
        {
            ["name"] = nameof(AzureADSettings),
            ["AzureADSettings"] = JObject.FromObject(azureADSettings),
        });
    }
}
