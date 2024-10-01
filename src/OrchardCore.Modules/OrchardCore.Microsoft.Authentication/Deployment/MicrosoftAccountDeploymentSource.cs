using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Deployment;

public sealed class MicrosoftAccountDeploymentSource : IDeploymentSource
{
    private readonly IMicrosoftAccountService _microsoftAccountService;

    public MicrosoftAccountDeploymentSource(IMicrosoftAccountService microsoftAccountService)
    {
        _microsoftAccountService = microsoftAccountService;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not MicrosoftAccountDeploymentStep)
        {
            return;
        }

        var microsoftAccountSettings = await _microsoftAccountService.GetSettingsAsync();

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Settings",
            [nameof(MicrosoftAccountSettings)] = JObject.FromObject(microsoftAccountSettings),
        });
    }
}
