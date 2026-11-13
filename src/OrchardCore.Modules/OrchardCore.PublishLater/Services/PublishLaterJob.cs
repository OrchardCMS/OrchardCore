using OrchardCore.ContentManagement;
using OrchardCore.Jobs;
using OrchardCore.Modules;
using OrchardCore.PublishLater.Models;

namespace OrchardCore.PublishLater.Services;

/// <summary>
/// The parameters of a run of <see cref="PublishLaterJob"/>.
/// </summary>
public sealed class PublishLaterJobParameters
{
    /// <summary>
    /// Gets or sets the identifier of the content item to publish.
    /// </summary>
    public string ContentItemId { get; set; }
}

/// <summary>
/// Publishes a content item whose scheduled publish date arrived. A run is queued for each such content item, and
/// executed once whatever the number of servers, so the content item is published once.
/// </summary>
public sealed class PublishLaterJob : IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public const string Name = "PublishLater";

    private readonly IContentManager _contentManager;
    private readonly IClock _clock;

    public PublishLaterJob(IContentManager contentManager, IClock clock)
    {
        _contentManager = contentManager;
        _clock = clock;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var parameters = context.GetParameters<PublishLaterJobParameters>();
        var contentItem = await _contentManager.GetAsync(parameters.ContentItemId, VersionOptions.Latest);

        // The schedule was changed or cancelled, or the content item was published or deleted, since the run was queued.
        if (contentItem is null
            || contentItem.Published
            || !contentItem.TryGet<PublishLaterPart>(out var part)
            || part.ScheduledPublishUtc is null
            || part.ScheduledPublishUtc > _clock.UtcNow)
        {
            context.LogInformation("The content item is not due to be published anymore.");

            return;
        }

        part.ScheduledPublishUtc = null;
        part.Apply();

        await _contentManager.PublishAsync(contentItem);

        context.LogInformation($"The content item '{contentItem.DisplayText}' was published.");
    }
}
