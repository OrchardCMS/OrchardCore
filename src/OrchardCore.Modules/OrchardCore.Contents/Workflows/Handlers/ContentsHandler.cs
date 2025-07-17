using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Workflows;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Handlers;

public class ContentsHandler : ContentHandlerBase
{
    private Dictionary<(string name, string id), ContentItem> _contentItemEvents;
    private bool _taskAdded;

    public override Task CreatedAsync(CreateContentContext context)
        => AddEventAsync(nameof(ContentCreatedEvent), context.ContentItem);

    public override Task UpdatedAsync(UpdateContentContext context)
        => AddEventAsync(nameof(ContentUpdatedEvent), context.ContentItem);

    public override Task DraftSavedAsync(SaveDraftContentContext context)
        => AddEventAsync(nameof(ContentDraftSavedEvent), context.ContentItem);

    public override Task PublishedAsync(PublishContentContext context)
        => AddEventAsync(nameof(ContentPublishedEvent), context.ContentItem);

    public override Task UnpublishedAsync(PublishContentContext context)
        => AddEventAsync(nameof(ContentUnpublishedEvent), context.ContentItem);

    public override Task RemovedAsync(RemoveContentContext context)
        => AddEventAsync(nameof(ContentDeletedEvent), context.ContentItem);

    public override Task VersionedAsync(VersionContentContext context)
        => AddEventAsync(nameof(ContentVersionedEvent), context.ContentItem);

    private Task AddEventAsync(string name, ContentItem contentItem)
    {
        _contentItemEvents ??= [];
        _contentItemEvents[(name, contentItem.ContentItemId)] = contentItem;

        AddDeferredTask();

        return Task.CompletedTask;
    }

    // We use a deferred task to ensure that the workflow events are triggered after all content handlers have been executed.
    private void AddDeferredTask()
    {
        if (_taskAdded)
        {
            return;
        }

        _taskAdded = true;

        // Using a local variable prevents the lambda from holding a ref on this scoped service.
        var contentItemEvents = _contentItemEvents;

        ShellScope.AddDeferredTask(scope => TriggerWorkflowEventsAsync(scope, contentItemEvents));
    }

    private static async Task TriggerWorkflowEventsAsync(ShellScope scope, Dictionary<(string name, string id), ContentItem> contentItemEvents)
    {
        var workflowManager = scope.ServiceProvider.GetRequiredService<IWorkflowManager>();

        foreach (var kvp in contentItemEvents)
        {
            await workflowManager.TriggerEventAsync(kvp.Key.name, new Dictionary<string, object>
            {
                { ContentEventConstants.ContentItemInputKey, kvp.Value },
                { ContentEventConstants.ContentEventInputKey, new ContentEventContext
                    {
                        Name = kvp.Key.name,
                        ContentType = kvp.Value.ContentType,
                        ContentItemId = kvp.Value.ContentItemId,
                        ContentItemVersionId = kvp.Value.ContentItemVersionId,
                    }
                },
            }, correlationId: kvp.Value.ContentItemId);
        }
    }
}
