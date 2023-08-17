using System;
using System.Collections.Generic;
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
            if (step is not MediaDeploymentStep mediaStep)
            {
                return;
            }

            IAsyncEnumerable<string> paths = null;

            if (mediaStep.IncludeAll)
            {
                paths = _mediaFileStore.GetDirectoryContentAsync(null, true).Where(e => !e.IsDirectory).Select(e => e.Path);
            }
            else
            {
                paths = new List<string>(mediaStep.FilePaths ?? Array.Empty<string>()).ToAsyncEnumerable();

                foreach (var directoryPath in mediaStep.DirectoryPaths ?? Array.Empty<string>())
                {
                    paths = paths.Concat(_mediaFileStore.GetDirectoryContentAsync(directoryPath, true).Where(e => !e.IsDirectory).Select(e => e.Path));
                }

                paths = paths.OrderBy(p => p);
            }

            var output = await paths.Select(p => new MediaDeploymentStepModel { SourcePath = p, TargetPath = p }).ToArrayAsync();

            foreach (var path in output)
            {
                var stream = await _mediaFileStore.GetFileStreamAsync(path.SourcePath);

                await result.FileBuilder.SetFileAsync(path.SourcePath, stream);
            }

            // Adding media files
            result.Steps.Add(new JObject(
                new JProperty("name", "media"),
                new JProperty("Files", JArray.FromObject(output))
            ));
        }

        private class MediaDeploymentStepModel
        {
            public string SourcePath { get; set; }
            public string TargetPath { get; set; }
        }
    }
}
