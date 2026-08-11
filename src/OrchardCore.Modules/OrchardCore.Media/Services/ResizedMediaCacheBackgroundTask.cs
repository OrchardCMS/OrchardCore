using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;

namespace OrchardCore.Media.Services;

[BackgroundTask(Schedule = "0 0 * * *", Description = "Resized media cache cleanup.")]
public sealed class ResizedMediaCacheBackgroundTask : IBackgroundTask
{
    private readonly ILogger _logger;

    public ResizedMediaCacheBackgroundTask(ILogger<ResizedMediaCacheBackgroundTask> logger)
    {
        _logger = logger;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;

        if (!mediaOptions.ResizedCacheMaxStale.HasValue)
        {
            return;
        }

        var cache = serviceProvider.GetRequiredService<IResizedImageCache>();

        try
        {
            await cache.ClearStaleAsync(mediaOptions.ResizedCacheMaxStale.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean the resized media cache.");
        }
    }
}
