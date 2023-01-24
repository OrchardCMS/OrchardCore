using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Facebook.Deployment
{
    public class FacebookLoginDeploymentSource : IDeploymentSource
    {
        private readonly FacebookSettings _facebookSettings;

        public FacebookLoginDeploymentSource(IOptions<FacebookSettings> facebookSettings)
        {
            _facebookSettings = facebookSettings.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var facebookLoginStep = step as FacebookLoginDeploymentStep;

            if (facebookLoginStep == null)
            {
                await Task.CompletedTask;
            }

            result.Steps.Add(new JObject(
                new JProperty("name", "FacebookLogin"),
                new JProperty("FacebookLogin", JObject.FromObject(_facebookSettings))
            ));
        }
    }
}
