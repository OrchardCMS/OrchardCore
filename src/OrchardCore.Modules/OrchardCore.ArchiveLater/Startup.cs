using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.ArchiveLater.Drivers;
using OrchardCore.ArchiveLater.Handlers;
using OrchardCore.ArchiveLater.Indexes;
using OrchardCore.ArchiveLater.Models;
using OrchardCore.ArchiveLater.Services;
using OrchardCore.ArchiveLater.ViewModels;

namespace OrchardCore.ArchiveLater
{
    public class Startup : StartupBase
    {

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<ArchiveLaterPartViewModel>();
            });

            services
                .AddContentPart<ArchiveLaterPart>()
                .UseDisplayDriver<ArchiveLaterPartDisplayDriver>()
                .AddHandler<ArchiveLaterPartHandler>();

            services.AddScoped<IDataMigration, Migrations>();

            services.AddScoped<ArchiveLaterPartIndexProvider>();
            services.AddScoped<IScopedIndexProvider>(sp => sp.GetRequiredService<ArchiveLaterPartIndexProvider>());
            services.AddScoped<IContentHandler>(sp => sp.GetRequiredService<ArchiveLaterPartIndexProvider>());

            services.AddSingleton<IBackgroundTask, ScheduledArchivingBackgroundTask>();
        }
    }
}
