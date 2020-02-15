using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Settings;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Deployment
{
    public class LoginSettingsDeploymentSource : IDeploymentSource
    {
        private readonly IMembershipService _membershipService;
        private readonly ISiteService _siteService;

        public LoginSettingsDeploymentSource(IMembershipService membershipService, ISiteService siteService)
        {
            _membershipService = membershipService;
            _siteService = siteService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var loginSettingsStep = step as LoginSettingsDeploymentStep;

            if (loginSettingsStep == null)
            {
                return;
            }

            var loginSettings = await _membershipService.GetLoginSettingsAsync();

            // Adding Login settings
            result.Steps.Add(new JObject(
                new JProperty("name", "Settings"),
                new JProperty("LoginSettings", JObject.FromObject(loginSettings))
            ));
        }
    }
}
