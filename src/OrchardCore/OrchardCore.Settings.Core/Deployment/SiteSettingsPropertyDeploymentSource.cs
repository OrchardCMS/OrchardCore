using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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

            result.Steps.Add(new JObject(
                new JProperty("name", "Settings"),
                new JProperty(typeof(TModel).Name, JObject.FromObject(siteSettings.As<TModel>()))
            ));
        }
    }
}
