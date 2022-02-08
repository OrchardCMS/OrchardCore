using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundJobs;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.BackgroundJobs.Events;
using OrchardCore.BackgroundJobs.Handlers;
using OrchardCore.BackgroundJobs.Indexes;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.BackgroundJobs.Services;
using OrchardCore.Modules;
using YesSql.Indexes;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement;
using OrchardCore.BackgroundJobs.ViewModels;
using YesSql.Filters.Query;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.BackgroundJobs.Drivers;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Admin;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundJobs.Controllers;
using OrchardCore.Mvc.Core.Utilities;

namespace OrchardCore.BackgroundJobs
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }


        public override void ConfigureServices(IServiceCollection services)
        {
            // Infrastucture
            services
                .AddSingleton<IIndexProvider, BackgroundJobIndexProvider>()
                .AddSingleton<IBackgroundJobEntries, BackgroundJobEntries>()
                .AddSingleton<IBackgroundJobIdGenerator, BackgroundJobIdGenerator>()
                .AddScoped<IBackgroundJobExecutor, DefaultBackgroundJobExecutor>()
                .AddScoped<IBackgroundJobHandlerFactory, BackgroundJobHandlerFactory>()
                .AddScoped<IScheduleBuilderFactory, ScheduleBuilderFactory>()
                .AddScoped<IBackgroundJobService, BackgroundJobService>()
                .AddScoped<IBackgroundTask, ScheduleJobsBackgroundTask>()
                .AddScoped<IBackgroundJobStore, BackgroundJobStore>()
                .AddScoped<IBackgroundJobSession, BackgroundJobSession>()
                .AddScoped<IBackgroundJobEvent, BackgroundJobEventHandler>()
                .AddScoped<IBackgroundJobScheduleHandler, BackgroundJobScheduleHandler>()
                .AddScoped<IPermissionProvider, Permissions>()
                .AddScoped<IDataMigration, Migrations>();

            // Display
            services
                .AddScoped<INavigationProvider, AdminMenu>()
                .AddScoped<IDisplayManager<BackgroundJobExecution>, DisplayManager<BackgroundJobExecution>>()
                .AddScoped<IDisplayManager<BackgroundJobIndexOptions>, DisplayManager<BackgroundJobIndexOptions>>()
                .AddScoped<IDisplayDriver<BackgroundJobIndexOptions>, BackgroundJobOptionsDisplayDriver>()
                .AddScoped<IDisplayDriver<BackgroundJobExecution>, BackgroundJobScheduleDisplayDriver>()
                .AddScoped<IDisplayDriver<BackgroundJobExecution>, RepeatCrontabScheduleDisplayDriver>()
                .AddScoped<IDisplayDriver<BackgroundJobExecution>, BackgroundJobFieldsDisplayDriver>()
                .AddScoped<IDisplayDriver<BackgroundJobExecution>, LogJobDisplayDriver>()
                .AddTransient<IBackgroundJobsAdminListFilterProvider, DefaultBackgroundJobsAdminListFilterProvider>()
                .AddScoped<IBackgroundJobsAdminListQueryService, DefaultBackgroundJobsAdminListQueryService>()
                .AddSingleton<IBackgroundJobsAdminListFilterParser>(sp =>
                {
                    var filterProviders = sp.GetServices<IBackgroundJobsAdminListFilterProvider>();
                    var builder = new QueryEngineBuilder<BackgroundJobExecution>();
                    foreach (var provider in filterProviders)
                    {
                        provider.Build(builder);
                    }

                    var parser = builder.Build();

                    return new DefaultBackgroundJobsAdminListFilterParser(parser);
                });

            // Jobs

            services.AddBackgroundJob<LogJob2, LogJobHandler>(o =>
            {
                o.WithDisplayName<Startup>(S => S["Log Job"]);
                o.RetryAttempts = 10;
                o.ConcurrentJobsLimit = 0;
            });
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {

            // Option controller
            var optionControllerName = typeof(BackgroundJobOptionController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "JobOptionIndex",
                areaName: "OrchardCore.BackgroundJobs",
                pattern: _adminOptions.AdminUrlPrefix + "/BackgroundJobs",
                defaults: new { controller = optionControllerName, action = nameof(BackgroundJobOptionController.Index) }
            );

            var executionControllerName = typeof(ExecutionController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "JobExecutionIndex",
                areaName: "OrchardCore.BackgroundJobs",
                pattern: _adminOptions.AdminUrlPrefix + "/BackgroundJobs/{name}",
                defaults: new { controller = executionControllerName, action = nameof(ExecutionController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "JobExecutionDelete",
                areaName: "OrchardCore.BackgroundJobs",
                pattern: _adminOptions.AdminUrlPrefix + "/BackgroundJobs/Delete/{id}/{name}",
                defaults: new { controller = executionControllerName, action = nameof(ExecutionController.Delete) }
            );

            routes.MapAreaControllerRoute(
                name: "JobExecutionCancel",
                areaName: "OrchardCore.BackgroundJobs",
                pattern: _adminOptions.AdminUrlPrefix + "/BackgroundJobs/Cancel/{id}/{name}",
                defaults: new { controller = executionControllerName, action = nameof(ExecutionController.Cancel) }
            );

            routes.MapAreaControllerRoute(
                name: "JobExecutionExecuteNow",
                areaName: "OrchardCore.BackgroundJobs",
                pattern: _adminOptions.AdminUrlPrefix + "/BackgroundJobs/ExecuteNow/{id}/{name}",
                defaults: new { controller = executionControllerName, action = nameof(ExecutionController.ExecuteNow) }
            );


            var jobService = serviceProvider.GetService<IBackgroundJobService>();

            for (var i = 0; i < 3; i++)
            {
                jobService.CreateJobAsync(new LogJob2
                {
                    Message = "test log... {0}"
                },
                jobService.Schedule.Now()
                .Repeat("* * * * *")

                ).GetAwaiter().GetResult();
            }

        }
    }
}
