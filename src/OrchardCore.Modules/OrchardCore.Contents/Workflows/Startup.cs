using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.Drivers;
using OrchardCore.Contents.Workflows.Handlers;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows
{
    [RequireFeatures("OrchardCore.Workflows")]
    public class Startup : StartupBase
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

            services.AddScoped<IContentHandler, ContentsHandler>();
            services.AddScoped<IWorkflowValueSerializer, ContentItemSerializer>();
        }
    }
}
