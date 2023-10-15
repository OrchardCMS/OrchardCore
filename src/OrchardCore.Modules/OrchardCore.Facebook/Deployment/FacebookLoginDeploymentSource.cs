using System.Text.Json;
using System.Threading.Tasks;
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

            if (facebookLoginStep == null || await _facebookService.GetSettingsAsync() is not { } settings)
            {
                return;
            }

            var jObject = JsonSerializer.SerializeToNode(settings)!.AsObject();

            // The 'name' property should match the related recipe step name.
            jObject["name"] = "FacebookCoreSettings";

            result.Steps.Add(jObject);
        }
    }
}
