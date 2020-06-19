using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Deployment
{
    public class LoginSettingsDeploymentSource : IDeploymentSource
    {
        private readonly ISiteService _siteService;

        public LoginSettingsDeploymentSource(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var loginSettingsStep = step as LoginSettingsDeploymentStep;

            if (loginSettingsStep == null)
            {
                return;
            }

            var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

            // Adding Login settings
            result.Steps.Add(new JObject(
                new JProperty("name", "Settings"),
                new JProperty("LoginSettings", JObject.FromObject(loginSettings))
            ));
        }
    }
}
