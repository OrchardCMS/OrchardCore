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

            List<string> paths;

            if (mediaStep.IncludeAll)
            {
                var fileStoreEntries = await _mediaFileStore.GetDirectoryContentAsync(null, true);

                paths =
                (
                    from fileStoreEntry in fileStoreEntries
                    where !fileStoreEntry.IsDirectory
                    select fileStoreEntry.Path
                ).ToList();
            }
            else
            {
                paths = new List<string>(mediaStep.FilePaths ?? Array.Empty<string>());

                foreach (var directoryPath in mediaStep.DirectoryPaths ?? Array.Empty<string>())
                {
                    var fileStoreEntries = await _mediaFileStore.GetDirectoryContentAsync(directoryPath, true);

                    paths.AddRange(
                        from fileStoreEntry in fileStoreEntries
                        where !fileStoreEntry.IsDirectory
                        select fileStoreEntry.Path);
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
