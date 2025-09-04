using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.Drivers;
using OrchardCore.Contents.Workflows.Handlers;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows;

[RequireFeatures("OrchardCore.Workflows")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<ContentCreatedEvent, ContentCreatedEventDisplayDriver>();
        services.AddActivity<ContentDeletedEvent, ContentDeletedEventDisplayDriver>();
        services.AddActivity<ContentPublishedEvent, ContentPublishedEventDisplayDriver>();
        services.AddActivity<ContentUnpublishedEvent, ContentUnpublishedEventDisplayDriver>();
        services.AddActivity<ContentUpdatedEvent, ContentUpdatedEventDisplayDriver>();
        services.AddActivity<ContentDraftSavedEvent, ContentDraftSavedEventDisplayDriver>();
        services.AddActivity<ContentVersionedEvent, ContentVersionedEventDisplayDriver>();
        services.AddActivity<DeleteContentTask, DeleteContentTaskDisplayDriver>();
        services.AddActivity<PublishContentTask, PublishContentTaskDisplayDriver>();
        services.AddActivity<UnpublishContentTask, UnpublishContentTaskDisplayDriver>();
        services.AddActivity<CreateContentTask, CreateContentTaskDisplayDriver>();
        services.AddActivity<RetrieveContentTask, RetrieveContentTaskDisplayDriver>();
        services.AddActivity<UpdateContentTask, UpdateContentTaskDisplayDriver>();

        services.AddScoped<IWorkflowValueSerializer, ContentItemSerializer>();
    }
}

[RequireFeatures("OrchardCore.Workflows")]
public sealed class ContentHandlerStartup : StartupBase
{
    // The order is set to OrchardCoreConstants.ConfigureOrder.WorkflowsContentHandlers to ensure the workflows content
    // handler is registered first in the DI container. This causes the workflows content handler to be invoked last
    // when content events are triggered, allowing it to access the final state of the content item after all other
    // content handlers have executed. Note: handlers are resolved in reverse order, so setting this constant ensures
    // this handler runs last during content item created, updated, etc. events.
    public override int Order => OrchardCoreConstants.ConfigureOrder.WorkflowsContentHandlers;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentHandler, ContentsHandler>();
    }
}
