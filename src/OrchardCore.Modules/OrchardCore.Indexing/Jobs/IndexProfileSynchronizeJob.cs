using Microsoft.Extensions.Logging;
using OrchardCore.Indexing.Models;
using OrchardCore.Jobs;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Jobs;

/// <summary>
/// The parameters of a run of <see cref="IndexProfileSynchronizeJob"/>.
/// </summary>
public sealed class IndexProfileSynchronizeJobParameters
{
    /// <summary>
    /// Gets or sets the identifier of the index to synchronize.
    /// </summary>
    public string IndexProfileId { get; set; }
}

/// <summary>
/// Synchronizes an index with its source, such as after it was reset or rebuilt from the admin, without making the
/// request wait for it. The run is executed by the server that queued it, as the index of some providers is kept
/// on the disk of each server.
/// </summary>
public sealed class IndexProfileSynchronizeJob : IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public const string Name = "IndexSynchronize";

    private readonly IIndexProfileManager _indexProfileManager;
    private readonly IEnumerable<IIndexProfileHandler> _handlers;
    private readonly ILogger _logger;

    public IndexProfileSynchronizeJob(
        IIndexProfileManager indexProfileManager,
        IEnumerable<IIndexProfileHandler> handlers,
        ILogger<IndexProfileSynchronizeJob> logger)
    {
        _indexProfileManager = indexProfileManager;
        _handlers = handlers;
        _logger = logger;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var parameters = context.GetParameters<IndexProfileSynchronizeJobParameters>();
        var indexProfile = await _indexProfileManager.FindByIdAsync(parameters.IndexProfileId);

        if (indexProfile is null)
        {
            context.LogInformation("The index doesn't exist anymore.");

            return;
        }

        context.ReportProgress(null, $"Synchronizing the index '{indexProfile.Name}'.");

        var failures = 0;

        // Each handler synchronizes the indexes it knows. Their failures are counted, to fail the run.
        foreach (var handler in _handlers)
        {
            try
            {
                await handler.SynchronizedAsync(new IndexProfileSynchronizedContext(indexProfile));
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                failures++;
                _logger.LogError(ex, "Error synchronizing the index '{IndexName}'.", indexProfile.Name);
                context.LogError($"{handler.GetType().Name}: {ex.Message}");
            }
        }

        if (failures > 0)
        {
            throw new InvalidOperationException($"The index '{indexProfile.Name}' couldn't be synchronized. The indexing background task keeps synchronizing it.");
        }

        context.LogInformation($"The index '{indexProfile.Name}' was synchronized.");
    }
}
