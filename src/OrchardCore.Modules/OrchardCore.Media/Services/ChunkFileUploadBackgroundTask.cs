using System;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.BackgroundTasks;

namespace OrchardCore.Media.Services;

[BackgroundTask(Schedule = "0 0 * * *", Description = "Checks for abandoned file uploads.")]
public class ChunkFileUploadBackgroundTask : IBackgroundTask
{
    private readonly IChunkFileUploadService _chunkFileUploadService;

    public ChunkFileUploadBackgroundTask(IChunkFileUploadService chunkFileUploadService) =>
        _chunkFileUploadService = chunkFileUploadService;

    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        _chunkFileUploadService.PurgeTempDirectory();

        return Task.CompletedTask;
    }
}
