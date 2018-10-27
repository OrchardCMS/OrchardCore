using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Media.Deployment
{
    public class MediaDeploymentSource : IDeploymentSource
    {
        private readonly IMediaFileStore _mediaFileStore;

        public MediaDeploymentSource(IMediaFileStore mediaFileStore)
        {
            _mediaFileStore = mediaFileStore;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (!(step is MediaDeploymentStep mediaStep))
            {
                return;
            }

            var paths = mediaStep.IncludeAll
                ? (from fileStoreEntry in await _mediaFileStore.GetDirectoryContentAsync(null, true)
                   where !fileStoreEntry.IsDirectory
                   select fileStoreEntry.Path).ToArray()
                : mediaStep.Paths;

            foreach (var path in paths)
            {
                var stream = await _mediaFileStore.GetFileStreamAsync(path);

                await result.FileBuilder.SetFileAsync(path, stream);
            }

            // Adding media files
            result.Steps.Add(new JObject(
                new JProperty("name", "media"),
                new JProperty("Paths", JArray.FromObject(paths)
            )));
        }
    }
}