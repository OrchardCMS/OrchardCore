using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ArchiveLater.Drivers;
using OrchardCore.ArchiveLater.Indexes;
using OrchardCore.ArchiveLater.Models;
using OrchardCore.ArchiveLater.Services;
using OrchardCore.ArchiveLater.ViewModels;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;

namespace OrchardCore.ArchiveLater;

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
            .UseDisplayDriver<ArchiveLaterPartDisplayDriver>();

        services.AddDataMigration<Migrations>();

        services.AddScoped<ArchiveLaterPartIndexProvider>();
        services.AddScoped<IScopedIndexProvider>(sp => sp.GetRequiredService<ArchiveLaterPartIndexProvider>());
        services.AddScoped<IContentHandler>(sp => sp.GetRequiredService<ArchiveLaterPartIndexProvider>());

        services.AddSingleton<IBackgroundTask, ScheduledArchivingBackgroundTask>();
    }
}
