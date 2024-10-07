using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Deployment;

public class AzureADDeploymentSource : IDeploymentSource
{
    private readonly IAzureADService _azureADService;

    public AzureADDeploymentSource(IAzureADService azureADService)
    {
        _azureADService = azureADService;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not AzureADDeploymentStep azureADStep)
        {
            return;
        }

        var azureADSettings = await _azureADService.GetSettingsAsync();

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Settings",
            [nameof(AzureADSettings)] = JObject.FromObject(azureADSettings),
        });
    }
}
