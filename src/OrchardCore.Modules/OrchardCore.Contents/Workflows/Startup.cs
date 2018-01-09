using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.Drivers;
using OrchardCore.Contents.Workflows.Handlers;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

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
            services.AddActivity<ContentUpdatedEvent, ContentUpdatedEventDisplay>();
            services.AddActivity<ContentVersionedEvent, ContentVersionedEventDisplay>();
            services.AddActivity<DeleteContentTask, DeleteContentTaskDisplay>();
            services.AddActivity<PublishContentTask, PublishContentTaskDisplay>();

            services.AddScoped<IContentHandler, ContentsHandler>();
        }
    }
}
