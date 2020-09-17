using Microsoft.Extensions.DependencyInjection;
using Fluid;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.PublishLater.Drivers;
using OrchardCore.PublishLater.Handlers;
using OrchardCore.PublishLater.Indexes;
using OrchardCore.PublishLater.Models;
using OrchardCore.PublishLater.Services;
using OrchardCore.PublishLater.ViewModels;
using YesSql.Indexes;

namespace OrchardCore.PublishLater
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<PublishLaterPartViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services
                .AddContentPart<PublishLaterPart>()
                .UseDisplayDriver<PublishLaterPartDisplayDriver>()
                .AddHandler<PublishLaterPartHandler>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddSingleton<IIndexProvider, PublishLaterPartIndexProvider>();

            services.AddSingleton<IBackgroundTask, ScheduledPublishingBackgroundTask>();
        }
    }
}
