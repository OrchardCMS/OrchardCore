using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Drivers;
using OrchardCore.Workflows.Http.Filters;
using OrchardCore.Workflows.Http.Handlers;
using OrchardCore.Workflows.Http.Routing;
using OrchardCore.Workflows.Http.Scripting;
using OrchardCore.Workflows.Http.Services;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http
{
    [Feature("OrchardCore.Workflows.Http")]
    [RequireFeatures("OrchardCore.Workflows")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(WorkflowActionFilter));
            });

            services.AddScoped<IWorkflowDefinitionEventHandler, WorkflowDefinitionRoutesHandler>();
            services.AddScoped<IWorkflowInstanceHandler, WorkflowInstanceRoutesHandler>();

            services.AddSingleton<IWorkflowDefinitionRouteEntries, WorkflowDefinitionRouteEntries>();
            services.AddSingleton<IWorkflowInstanceRouteEntries, WorkflowInstanceRouteEntries>();
            services.AddSingleton<IWorkflowDefinitionPathEntries, WorkflowDefinitionPathEntries>();
            services.AddSingleton<IWorkflowInstancePathEntries, WorkflowInstancePathEntries>();
            services.AddSingleton<IGlobalMethodProvider, HttpContextMethodProvider>();

            services.AddActivity<HttpRequestEvent, HttpRequestEventDisplay>();
            services.AddActivity<HttpRequestFilterEvent, HttpRequestFilterEventDisplay>();
            services.AddActivity<HttpRedirectTask, HttpRedirectTaskDisplay>();
            services.AddActivity<HttpRequestTask, HttpRequestTaskDisplay>();
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
                where workflowDefinitionDictionary.ContainsKey(workflowInstance.DefinitionId)
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
                where workflowDefinitionDictionary.ContainsKey(workflowInstance.DefinitionId)
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
