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
            services.AddActivity<ContentCreatedEvent, ContentCreatedEventDisplay>();
            services.AddActivity<ContentDeletedEvent, ContentDeletedEventDisplay>();
            services.AddActivity<ContentPublishedEvent, ContentPublishedEventDisplay>();
            services.AddActivity<ContentUnpublishedEvent, ContentUnpublishedEventDisplay>();
            services.AddActivity<ContentUpdatedEvent, ContentUpdatedEventDisplay>();
            services.AddActivity<ContentVersionedEvent, ContentVersionedEventDisplay>();
            services.AddActivity<DeleteContentTask, DeleteContentTaskDisplay>();
            services.AddActivity<PublishContentTask, PublishContentTaskDisplay>();
            services.AddActivity<UnpublishContentTask, UnpublishContentTaskDisplay>();
            services.AddActivity<CreateContentTask, CreateContentTaskDisplay>();
            services.AddActivity<RetrieveContentTask, RetrieveContentTaskDisplay>();
            services.AddActivity<UpdateContentTask, UpdateContentTaskDisplay>();

            services.AddScoped<IContentHandler, ContentsHandler>();
            services.AddScoped<IWorkflowValueSerializer, ContentItemSerializer>();
        }
    }
}
