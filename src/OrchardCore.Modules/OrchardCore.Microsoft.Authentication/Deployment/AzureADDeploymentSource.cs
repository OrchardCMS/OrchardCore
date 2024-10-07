using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Deployment;

public class AzureADDeploymentSource : DeploymentSourceBase<AzureADDeploymentStep>
{
    private readonly IAzureADService _azureADService;

    public AzureADDeploymentSource(IAzureADService azureADService)
    {
        _azureADService = azureADService;
    }

    public override async Task ProcessDeploymentStepAsync(DeploymentPlanResult result)
    {
        var azureADSettings = await _azureADService.GetSettingsAsync();

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Settings",
            [nameof(AzureADSettings)] = JObject.FromObject(azureADSettings),
        });
    }
}
