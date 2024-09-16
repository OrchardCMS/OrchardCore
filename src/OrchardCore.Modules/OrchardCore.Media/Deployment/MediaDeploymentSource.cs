using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Media.Deployment;

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
            paths = new List<string>(mediaStep.FilePaths ?? []).ToAsyncEnumerable();

            foreach (var directoryPath in mediaStep.DirectoryPaths ?? [])
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
        result.Steps.Add(new JsonObject
        {
            ["name"] = "media",
            ["Files"] = JArray.FromObject(output),
        });
    }

    private sealed class MediaDeploymentStepModel
    {
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
    }
}
