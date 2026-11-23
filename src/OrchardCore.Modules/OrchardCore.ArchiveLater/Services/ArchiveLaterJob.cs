using OrchardCore.ArchiveLater.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Jobs;
using OrchardCore.Modules;

namespace OrchardCore.ArchiveLater.Services;

/// <summary>
/// The parameters of a run of <see cref="ArchiveLaterJob"/>.
/// </summary>
public sealed class ArchiveLaterJobParameters
{
    /// <summary>
    /// Gets or sets the identifier of the content item to archive.
    /// </summary>
    public string ContentItemId { get; set; }
}

/// <summary>
/// Archives a content item whose scheduled archive date arrived. A run is queued for each such content item, and
/// executed once whatever the number of servers, so the content item is archived once.
/// </summary>
public sealed class ArchiveLaterJob : IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public const string Name = "ArchiveLater";

    private readonly IContentManager _contentManager;
    private readonly IClock _clock;

    public ArchiveLaterJob(IContentManager contentManager, IClock clock)
    {
        _contentManager = contentManager;
        _clock = clock;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var parameters = context.GetParameters<ArchiveLaterJobParameters>();
        var contentItem = await _contentManager.GetAsync(parameters.ContentItemId);

        // The schedule was changed or cancelled, or the content item was archived or deleted, since the run was queued.
        if (contentItem is null
            || !contentItem.TryGet<ArchiveLaterPart>(out var part)
            || part.ScheduledArchiveUtc is null
            || part.ScheduledArchiveUtc > _clock.UtcNow)
        {
            context.LogInformation("The content item is not due to be archived anymore.");

            return;
        }

        part.ScheduledArchiveUtc = null;
        part.Apply();

        await _contentManager.UnpublishAsync(contentItem);

        context.LogInformation($"The content item '{contentItem.DisplayText}' was archived.");
    }
}
