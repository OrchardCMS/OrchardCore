using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.PublishLater.Drivers;
using OrchardCore.PublishLater.Indexes;
using OrchardCore.PublishLater.Models;
using OrchardCore.PublishLater.Services;

namespace OrchardCore.PublishLater;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddContentPart<PublishLaterPart>()
            .UseDisplayDriver<PublishLaterPartDisplayDriver>();

        services.AddDataMigration<Migrations>();

        services.AddScoped<PublishLaterPartIndexProvider>();
        services.AddScoped<IScopedIndexProvider>(sp => sp.GetRequiredService<PublishLaterPartIndexProvider>());
        services.AddScoped<IContentHandler>(sp => sp.GetRequiredService<PublishLaterPartIndexProvider>());

        // The background task finds the content items that are due on every server; the job handles each of them once.
        services.AddSingleton<IBackgroundTask, ScheduledPublishingBackgroundTask>();
        services.AddJob<PublishLaterJob>(PublishLaterJob.Name, job =>
        {
            job.Title = PublishLaterLocalizationSources.JobTitle;
            job.Description = PublishLaterLocalizationSources.JobDescription;
        });
    }
}
