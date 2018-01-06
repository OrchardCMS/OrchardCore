using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Navigation;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Security.Permissions;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Drivers;
using OrchardCore.Workflows.Filters;
using OrchardCore.Workflows.Handlers;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Routing;
using OrchardCore.Workflows.Scripting;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.WorkflowContextProviders;
using YesSql.Indexes;

namespace OrchardCore.Workflows
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(WorkflowActionFilter));
            });

            services.AddScoped(typeof(Resolver<>));
            services.AddScoped<ISignalService, SignalService>();
            services.AddScoped<IActivityLibrary, ActivityLibrary>();
            services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
            services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
            services.AddScoped<IWorkflowManager, WorkflowManager>();
            services.AddScoped<IDisplayManager<IActivity>, DisplayManager<IActivity>>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();

            services.AddSingleton<IIndexProvider, WorkflowDefinitionIndexProvider>();
            services.AddSingleton<IIndexProvider, WorkflowInstanceIndexProvider>();

            services.AddScoped<IActivity, NotifyTask>();
            services.AddScoped<IActivity, SetVariableTask>();
            services.AddScoped<IActivity, SetOutputTask>();
            services.AddScoped<IActivity, CorrelateTask>();
            services.AddScoped<IActivity, EvaluateExpressionTask>();
            services.AddScoped<IActivity, BranchTask>();
            services.AddScoped<IActivity, ForLoopTask>();
            services.AddScoped<IActivity, WhileLoopTask>();
            services.AddScoped<IActivity, IfElseTask>();
            services.AddScoped<IActivity, DecisionTask>();
            services.AddScoped<IActivity, SignalEvent>();
            services.AddScoped<IActivity, EmailTask>();
            services.AddScoped<IActivity, HttpRequestEvent>();
            services.AddScoped<IActivity, HttpRequestFilterEvent>();
            services.AddScoped<IActivity, HttpRedirectTask>();

            services.AddScoped<IDisplayDriver<IActivity>, NotifyTaskDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, SetVariableTaskDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, SetOutputTaskDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, CorrelateTaskDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, EvaluateExpressionTaskDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, BranchTaskDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, ForLoopTaskDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, WhileLoopTaskDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, IfElseTaskDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, DecisionTaskDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, SignalEventDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, EmailTaskDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, HttpRequestEventDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, HttpRequestFilterEventDisplay>();
            services.AddScoped<IDisplayDriver<IActivity>, HttpRedirectTaskDisplay>();

            services.AddScoped<IWorkflowContextProvider, DefaultWorkflowContextProvider>();
            services.AddScoped<IWorkflowContextProvider, SignalWorkflowContextProvider>();

            services.AddScoped<IWorkflowDefinitionHandler, WorkflowDefinitionRoutesHandler>();
            services.AddScoped<IWorkflowInstanceHandler, WorkflowInstanceRoutesHandler>();

            services.AddSingleton<IWorkflowDefinitionRouteEntries, WorkflowDefinitionRouteEntries>();
            services.AddSingleton<IWorkflowInstanceRouteEntries, WorkflowInstanceRouteEntries>();
            services.AddSingleton<IWorkflowDefinitionPathEntries, WorkflowDefinitionPathEntries>();
            services.AddSingleton<IWorkflowInstancePathEntries, WorkflowInstancePathEntries>();
            services.AddSingleton<IGlobalMethodProvider, HttpContextMethodProvider>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var workflowDefinitionRepository = serviceProvider.GetRequiredService<IWorkflowDefinitionRepository>();
            var workflowInstanceRepository = serviceProvider.GetRequiredService<IWorkflowInstanceRepository>();
            var workflowDefinitionDictionary = workflowDefinitionRepository.ListAsync().GetAwaiter().GetResult().ToDictionary(x => x.Id);
            var workflowInstanceDictionary = workflowInstanceRepository.ListAsync().GetAwaiter().GetResult().ToDictionary(x => x.Id);

            ConfigureWorkflowRouteEntries(serviceProvider, workflowDefinitionDictionary, workflowInstanceDictionary);
            ConfigureWorkflowPathEntries(serviceProvider, workflowDefinitionDictionary, workflowInstanceDictionary);
            ConfigureWorkflowRoutes(routes);
        }

        private void ConfigureWorkflowRouteEntries(IServiceProvider serviceProvider, IDictionary<int, WorkflowDefinitionRecord> workflowDefinitionDictionary, IDictionary<int, WorkflowInstanceRecord> workflowInstanceDictionary)
        {
            var activityLibrary = serviceProvider.GetRequiredService<IActivityLibrary>();
            var workflowDefinitionRouteEntries = serviceProvider.GetRequiredService<IWorkflowDefinitionRouteEntries>();
            var workflowInstanceRouteEntries = serviceProvider.GetRequiredService<IWorkflowInstanceRouteEntries>();

            var workflowDefinitionRouteEntryQuery =
                from workflowDefinition in workflowDefinitionDictionary.Values
                from entry in WorkflowDefinitionRouteEntries.GetWorkflowDefinitionRoutesEntries(workflowDefinition, activityLibrary)
                select entry;

            var workflowInstanceRouteEntryQuery =
                from workflowInstance in workflowInstanceDictionary.Values
                let workflowDefinition = workflowDefinitionDictionary[workflowInstance.DefinitionId]
                from entry in WorkflowInstanceRouteEntries.GetWorkflowInstanceRoutesEntries(workflowInstance, workflowDefinition, activityLibrary)
                select entry;

            workflowDefinitionRouteEntries.AddEntries(workflowDefinitionRouteEntryQuery);
            workflowInstanceRouteEntries.AddEntries(workflowInstanceRouteEntryQuery);
        }

        private void ConfigureWorkflowPathEntries(IServiceProvider serviceProvider, IDictionary<int, WorkflowDefinitionRecord> workflowDefinitionDictionary, IDictionary<int, WorkflowInstanceRecord> workflowInstanceDictionary)
        {
            var activityLibrary = serviceProvider.GetRequiredService<IActivityLibrary>();
            var workflowDefinitionPathEntries = serviceProvider.GetRequiredService<IWorkflowDefinitionPathEntries>();
            var workflowInstancePathEntries = serviceProvider.GetRequiredService<IWorkflowInstancePathEntries>();

            var workflowDefinitionPathEntryQuery =
                from workflowDefinition in workflowDefinitionDictionary.Values
                from entry in WorkflowDefinitionPathEntries.GetWorkflowPathEntries(workflowDefinition, activityLibrary)
                select entry;

            var workflowInstancePathEntryQuery =
                from workflowInstance in workflowInstanceDictionary.Values
                let workflowDefinition = workflowDefinitionDictionary[workflowInstance.DefinitionId]
                from entry in WorkflowInstancePathEntries.GetWorkflowPathEntries(workflowInstance, workflowDefinition, activityLibrary)
                select entry;

            workflowDefinitionPathEntries.AddEntries(workflowDefinitionPathEntryQuery);
            workflowInstancePathEntries.AddEntries(workflowInstancePathEntryQuery);
        }

        private void ConfigureWorkflowRoutes(IRouteBuilder routes)
        {
            var workflowDefinitionRouter = new WorkflowDefinitionRouter(routes.DefaultHandler);
            var workflowInstanceRouter = new WorkflowInstanceRouter(routes.DefaultHandler);

            routes.Routes.Insert(0, workflowInstanceRouter);
            routes.Routes.Insert(0, workflowDefinitionRouter);
        }
    }
}
