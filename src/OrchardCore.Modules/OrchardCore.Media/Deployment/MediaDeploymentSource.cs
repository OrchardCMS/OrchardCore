using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Media.Models;

namespace OrchardCore.Media.Deployment
{
    public class MediaDeploymentSource : IDeploymentSource
    {
        private readonly IMediaFileStore _mediaFileStore;
        private readonly IAuthorizationService _authorizationService;

        public MediaDeploymentSource(
            IMediaFileStore mediaFileStore,
            IAuthorizationService authorizationService)
        {
            _mediaFileStore = mediaFileStore;
            _authorizationService = authorizationService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var mediaStep = step as MediaDeploymentStep;
            if (mediaStep == null)
            {
                return;
            }

            var paths = mediaStep.IncludeAll
                ? (from fileStoreEntry in await _mediaFileStore.GetDirectoryContentAsync(null, true)
                   where !fileStoreEntry.IsDirectory
                   select fileStoreEntry.Path).ToArray()
                : mediaStep.Paths;

            var files = new List<JObject>(paths.Length);

            foreach (var path in paths)
            {
                var base64 = await ReadFileAsBase64Async(path);

                var file = new JObject(
                    new JProperty("Path", path),
                    new JProperty("Base64", base64));

                files.Add(file);
            }

            // Adding media files
            result.Steps.Add(new JObject(
                new JProperty("name", "media"),
                new JProperty("Files", JArray.FromObject(files.ToArray()))
            ));
        }

        private async Task<string> ReadFileAsBase64Async(string path)
        {
            var fileInfo = await _mediaFileStore.GetFileInfoAsync(path);

            using (var fileStream = await _mediaFileStore.GetFileStreamAsync(path))
            {
                using (var memoryStream = new MemoryStream(Convert.ToInt32(fileInfo.Length)))
                {
                    var bytesRead = 0;
                    var buffer = new byte[1024];
                    do
                    {
                        bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);

                        if (bytesRead != 0)
                        {
                            await memoryStream.WriteAsync(buffer, 0, bytesRead);
                        }
                    }
                    while (bytesRead != 0);

                    await memoryStream.FlushAsync();
                    buffer = memoryStream.ToArray();

                    return Convert.ToBase64String(buffer);
                }
            }
        }
    }
}