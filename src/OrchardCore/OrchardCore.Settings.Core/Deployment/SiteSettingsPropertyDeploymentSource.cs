using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment;

public class SiteSettingsPropertyDeploymentSource<TModel> : IDeploymentSource where TModel : class, new()
{
    private readonly ISiteService _siteService;

    public SiteSettingsPropertyDeploymentSource(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not SiteSettingsPropertyDeploymentStep<TModel> settingsStep)
        {
            return;
        }

        var settingJPropertyName = typeof(TModel).Name;
        var settingJPropertyValue = JObject.FromObject(await _siteService.GetSettingsAsync<TModel>());

        var settingsStepJObject = result.Steps.FirstOrDefault(s => s["name"]?.ToString() == "Settings");
        if (settingsStepJObject != null)
        {
            settingsStepJObject.Add(settingJPropertyName, settingJPropertyValue);
        }
        else
        {
            result.Steps.Add(new JsonObject
            {
                ["name"] = "Settings",
                [settingJPropertyName] = settingJPropertyValue,
            });
        }
    }
}
