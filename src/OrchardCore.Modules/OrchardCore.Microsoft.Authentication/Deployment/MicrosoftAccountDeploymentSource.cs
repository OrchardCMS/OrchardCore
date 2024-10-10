using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Deployment;

public sealed class MicrosoftAccountDeploymentSource
    : DeploymentSourceBase<MicrosoftAccountDeploymentStep>
{
    private readonly IMicrosoftAccountService _microsoftAccountService;

    public MicrosoftAccountDeploymentSource(IMicrosoftAccountService microsoftAccountService)
    {
        _microsoftAccountService = microsoftAccountService;
    }

    protected override async Task ProcessAsync(MicrosoftAccountDeploymentStep step, DeploymentPlanResult result)
    {
        var microsoftAccountSettings = await _microsoftAccountService.GetSettingsAsync();

        result.Steps.Add(new JsonObject
        {
            ["name"] = nameof(MicrosoftAccountSettings),
            ["MicrosoftAccountSettings"] = JObject.FromObject(microsoftAccountSettings),
        });
    }
}
