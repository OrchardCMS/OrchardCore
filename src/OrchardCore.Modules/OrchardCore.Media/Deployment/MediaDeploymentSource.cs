using System;
using System.IO;
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
                var content = await ReadFileAsync(path);

                await result.FileBuilder.SetFileAsync(path, content);
            }

            // Adding media files
            result.Steps.Add(new JObject(
                new JProperty("name", "media"),
                new JProperty("Files", JArray.FromObject((from path in paths
                                                          select new JObject(
                                                              new JProperty("Path", path),
                                                              new JProperty("Base64", $"[file:base64('{path}')]")
                                                          )).ToArray())
            )));
        }

        private async Task<byte[]> ReadFileAsync(string path)
        {
            var fileInfo = await _mediaFileStore.GetFileInfoAsync(path);

            using (var fileStream = await _mediaFileStore.GetFileStreamAsync(path))
            {
                using (var memoryStream = new MemoryStream(Convert.ToInt32(fileInfo.Length)))
                {
                    int bytesRead;
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

                    return buffer;
                }
            }
        }
    }
}