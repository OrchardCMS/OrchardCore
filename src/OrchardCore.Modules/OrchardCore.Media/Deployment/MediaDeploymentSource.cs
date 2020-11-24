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
            if (!(step is MediaDeploymentStep mediaStep))
            {
                return;
            }

            List<string> paths = new List<string>();

            if (mediaStep.IncludeAll)
            {
                // var fileStoreEntries = await _mediaFileStore.GetDirectoryContentAsync(null, true);

                await foreach(var entry in _mediaFileStore.GetDirectoryContentAsync(null, true))
                {
                    if (entry.IsDirectory)
                    {
                        continue;
                    }              
                    paths.Add(entry.Path);
                }
            }
            else
            {
                paths = new List<string>(mediaStep.FilePaths ?? Array.Empty<string>());

                foreach (var directoryPath in mediaStep.DirectoryPaths ?? Array.Empty<string>())
                {
                    await foreach(var entry in _mediaFileStore.GetDirectoryContentAsync(directoryPath, true))
                    {
                        if (entry.IsDirectory)
                        {
                            continue;
                        }
                        paths.Add(entry.Path);
                    }
                }

                paths.Sort();
            }

            foreach (var path in paths)
            {
                var stream = await _mediaFileStore.GetFileStreamAsync(path);

                await result.FileBuilder.SetFileAsync(path, stream);
            }

            // Adding media files
            result.Steps.Add(new JObject(
                new JProperty("name", "media"),
                new JProperty("Files", JArray.FromObject(
                    (from path in paths
                     select new
                     {
                         SourcePath = path,
                         TargetPath = path
                     }).ToArray()
                ))
            ));
        }
    }
}
