using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.Drivers;
using OrchardCore.Contents.Workflows.Handlers;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows
{
    [RequireFeatures("OrchardCore.Workflows")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IActivity, ContentCreatedEvent>();
            services.AddScoped<IActivity, ContentDeletedEvent>();
            services.AddScoped<IActivity, ContentPublishedEvent>();
            services.AddScoped<IActivity, ContentUpdatedEvent>();
            services.AddScoped<IActivity, ContentVersionedEvent>();
            services.AddScoped<IActivity, DeleteContentTask>();
            services.AddScoped<IActivity, PublishContentTask>();

            services.AddScoped<IDisplayDriver<IActivity>, ContentCreatedEventDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, ContentDeletedEventDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, ContentPublishedEventDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, ContentUpdatedEventDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, ContentVersionedEventDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, DeleteContentTaskDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, PublishContentTaskDisplay>();

            services.AddScoped<IContentHandler, ContentsHandler>();
        }
    }
}
