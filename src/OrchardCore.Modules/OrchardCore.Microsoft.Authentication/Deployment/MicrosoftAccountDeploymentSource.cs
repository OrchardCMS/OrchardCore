using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Deployment;

public class MicrosoftAccountDeploymentSource : IDeploymentSource
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

        var settings = await _microsoftAccountService.GetSettingsAsync();

        var obj = new JsonObject { ["name"] = nameof(MicrosoftAccountSettings) };

        obj.Merge(JObject.FromObject(settings, JOptions.Default));

        result.Steps.Add(obj);
    }
}
