using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Deployment
{
    public class AllMediaProfilesDeploymentSource : IDeploymentSource
    {
        private readonly MediaProfilesManager _mediaProfilesManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AllMediaProfilesDeploymentSource(
            MediaProfilesManager mediaProfilesManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _mediaProfilesManager = mediaProfilesManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not AllMediaProfilesDeploymentStep)
            {
                return;
            }

            var mediaProfiles = await _mediaProfilesManager.GetMediaProfilesDocumentAsync();

            result.Steps.Add(new JsonObject
            {
                ["name"] = "MediaProfiles",
                ["MediaProfiles"] = JObject.FromObject(mediaProfiles.MediaProfiles, _jsonSerializerOptions),
            });
        }
    }
}
