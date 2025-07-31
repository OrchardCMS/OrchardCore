using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.PublishLater.Models;

namespace OrchardCore.PublishLater.Handlers;

/// <summary>
/// When a post was set to PublishLater, we want to set its Published date to the actual scheduled time,
/// not the time the website just happened to run the background task.
/// </summary>
public sealed class PublishLaterContentPartHandler : ContentPartHandler<PublishLaterPart>
{
    public override Task PublishingAsync(PublishContentContext context, PublishLaterPart part)
    {
        if (part.DoingScheduledPublish && part.ScheduledPublishUtc.HasValue)
        {
            context.ContentItem.PublishedUtc = part.ScheduledPublishUtc;
            part.ScheduledPublishUtc = null;
            part.DoingScheduledPublish = false;
            part.Apply();
        }

        return Task.CompletedTask;
    }
}
