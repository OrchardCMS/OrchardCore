using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Deployment
{
    public class AllMediaProfilesDeploymentSource : IDeploymentSource
    {
        private readonly MediaProfilesManager _mediaProfilesManager;

        public AllMediaProfilesDeploymentSource(MediaProfilesManager mediaProfilesManager)
        {
            _mediaProfilesManager = mediaProfilesManager;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allMediaProfilesStep = step as AllMediaProfilesDeploymentStep;

            if (allMediaProfilesStep == null)
            {
                return;
            }

            var mediaProfiles = await _mediaProfilesManager.GetMediaProfilesDocumentAsync();

            result.Steps.Add(new JObject(
                new JProperty("name", "MediaProfiles"),
                new JProperty("MediaProfiles", JObject.FromObject(mediaProfiles.MediaProfiles))
            ));
        }
    }
}
