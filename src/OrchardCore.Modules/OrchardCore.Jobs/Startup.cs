using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Jobs.BackgroundTasks;
using OrchardCore.Jobs.Drivers;
using OrchardCore.Jobs.Indexes;
using OrchardCore.Jobs.Migrations;
using OrchardCore.Jobs.Models;
using OrchardCore.Jobs.Services;
using OrchardCore.Jobs.ViewModels;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Jobs;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<JobExecutionOptions>(_shellConfiguration.GetSection("Jobs"));

        // The runs and the schedules have their own document table, so their many writes don't weigh on the others.
        services.Configure<StoreCollectionOptions>(options => options.Collections.Add(JobRun.Collection));
        services.AddDataMigration<JobsMigrations>();
        services.AddIndexProvider<JobRunIndexProvider>();
        services.AddIndexProvider<JobScheduleIndexProvider>();

        services.AddScoped<IJobManager, JobManager>();
        services.AddScoped<JobScheduleManager>();
        services.AddScoped<IJobScheduleManager>(serviceProvider => serviceProvider.GetRequiredService<JobScheduleManager>());
        services.AddScoped<JobRunExecutor>();
        services.AddSingleton<JobRunTracker>();
        services.AddSingleton<JobRunDispatcher>();
        services.AddSingleton<JobRunCoordinator>();
        services.AddSingleton<JobsState>();
        services.AddSingleton<IBackgroundTask, JobsBackgroundTask>();

        services.AddPermissionProvider<JobsPermissions>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddScoped<JobDisplayService>();

        // Build the rows of the runs, jobs and schedules admin lists.
        services.AddDisplayDriver<JobRun, JobRunDisplayDriver>();
        services.AddDisplayDriver<JobEntry, JobEntryDisplayDriver>();
        services.AddDisplayDriver<JobSchedule, JobScheduleDisplayDriver>();
    }
}
