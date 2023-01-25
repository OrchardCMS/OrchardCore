using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Facebook.Services;

namespace OrchardCore.Facebook.Deployment
{
    public class FacebookLoginDeploymentSource : IDeploymentSource
    {
        private readonly IFacebookService _facebookService;

        public FacebookLoginDeploymentSource(IFacebookService facebookService)
        {
            _facebookService = facebookService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var facebookLoginStep = step as FacebookLoginDeploymentStep;

            if (facebookLoginStep == null)
            {
                return;
            }

            var settings = await _facebookService.GetSettingsAsync();

            result.Steps.Add(new JObject(
                new JProperty("name", "FacebookLogin"),
                new JProperty("FacebookLogin", JObject.FromObject(settings))
            ));
        }
    }
}
