using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.PublishLater.Drivers;
using OrchardCore.PublishLater.Handlers;
using OrchardCore.PublishLater.Indexes;
using OrchardCore.PublishLater.Models;
using OrchardCore.PublishLater.Services;
using OrchardCore.PublishLater.ViewModels;
using OrchardCore.BackgroundJobs.Services;
using OrchardCore.BackgroundJobs.Events;

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
                .UseDisplayDriver<PublishLaterPartDisplayDriver>()
                .AddHandler<PublishLaterPartHandler>();

            services.AddScoped<IDataMigration, Migrations>();

            //services.AddScoped<PublishLaterPartIndexProvider>();
            //services.AddScoped<IScopedIndexProvider>(sp => sp.GetRequiredService<PublishLaterPartIndexProvider>());
            //services.AddScoped<IContentHandler>(sp => sp.GetRequiredService<PublishLaterPartIndexProvider>());

            //services.AddSingleton<IBackgroundTask, ScheduledPublishingBackgroundTask>();

            services
                .AddBackgroundJob<PublishLaterBackgroundJob, PublishLaterBackgroundJobHandler>(o =>
                {
                    o.WithDisplayName<Startup>(S => S["Publish content later"]);
                })
                .AddScoped<IDisplayDriver<BackgroundJobExecution>, PublishLaterBackgroundJobDisplayDriver>()
                .AddScoped<IBackgroundJobEvent, PublishLaterBackgroundJobEventHandler>();
        }
    }
}
