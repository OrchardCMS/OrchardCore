using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.PublishLater.Drivers;
using OrchardCore.PublishLater.Indexes;
using OrchardCore.PublishLater.Models;
using OrchardCore.PublishLater.Services;
using OrchardCore.PublishLater.ViewModels;

namespace OrchardCore.PublishLater
{
    public class Startup : StartupBase
    {

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<PublishLaterPartViewModel>();
            });

            services
                .AddContentPart<PublishLaterPart>()
                .UseDisplayDriver<PublishLaterPartDisplayDriver>();

            services.AddDataMigration<Migrations>();

            services.AddScoped<PublishLaterPartIndexProvider>();
            services.AddScoped<IScopedIndexProvider>(sp => sp.GetRequiredService<PublishLaterPartIndexProvider>());
            services.AddScoped<IContentHandler>(sp => sp.GetRequiredService<PublishLaterPartIndexProvider>());

            services.AddSingleton<IBackgroundTask, ScheduledPublishingBackgroundTask>();
        }
    }
}
