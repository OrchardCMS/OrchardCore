using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.Entities;

namespace OrchardCore.Settings.Deployment
{
    public class SiteSettingsPropertyDeploymentSource<TModel> : IDeploymentSource where TModel : class, new()
    {
        private readonly ISiteService _siteService;

        public SiteSettingsPropertyDeploymentSource(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var settingsStep = step as SiteSettingsPropertyDeploymentStep<TModel>;
            if (settingsStep == null)
            {
                return;
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var name = typeof(TModel).Name;
            var value = JsonSerializer.SerializeToNode(siteSettings.As<TModel>());

            var settingsStepJObject = result.Steps.FirstOrDefault(s => s["name"]?.ToString() == "Settings");
            if (settingsStepJObject != null)
            {
                settingsStepJObject[name] = value;
            }
            else
            {
                result.AddSimpleStep("Settings", name, value);
            }
        }
    }
}
