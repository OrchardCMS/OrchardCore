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

            // The 'name' property should match the related recipe step name.
            var jObject = new JObject(new JProperty("name", "FacebookCoreSettings"));

            // Merge settings as the recipe step doesn't use a child property.
            jObject.Merge(JObject.FromObject(settings));

            result.Steps.Add(jObject);
        }
    }
}
