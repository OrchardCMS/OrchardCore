using System.Text.Json;
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
        var settings = await _azureADService.GetSettingsAsync();

        var obj = new JsonObject { ["name"] = nameof(AzureADSettings) };

        obj.Merge(JObject.FromObject(settings, JOptions.Default));

        result.Steps.Add(obj);
    }
}
