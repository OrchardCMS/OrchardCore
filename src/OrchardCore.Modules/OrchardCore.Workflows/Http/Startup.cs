using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Drivers;
using OrchardCore.Workflows.Http.Filters;
using OrchardCore.Workflows.Http.Handlers;
using OrchardCore.Workflows.Http.Liquid;
using OrchardCore.Workflows.Http.Scripting;
using OrchardCore.Workflows.Http.Services;
using OrchardCore.Workflows.Http.WorkflowContextProviders;
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

            services.AddScoped<IWorkflowTypeEventHandler, WorkflowDefinitionRoutesHandler>();
            services.AddScoped<IWorkflowHandler, WorkflowInstanceRoutesHandler>();

            services.AddSingleton<IWorkflowDefinitionRouteEntries, WorkflowDefinitionRouteEntries>();
            services.AddSingleton<IWorkflowInstanceRouteEntries, WorkflowInstanceRouteEntries>();
            services.AddSingleton<IGlobalMethodProvider, HttpMethodsProvider>();
            services.AddScoped<IWorkflowExecutionContextHandler, SignalWorkflowExecutionContextHandler>();

            services.AddActivity<HttpRequestEvent, HttpRequestEventDisplay>();
            services.AddActivity<HttpRequestFilterEvent, HttpRequestFilterEventDisplay>();
            services.AddActivity<HttpRedirectTask, HttpRedirectTaskDisplay>();
            services.AddActivity<HttpRequestTask, HttpRequestTaskDisplay>();
            services.AddActivity<SignalEvent, SignalEventDisplay>();

            services.AddScoped<ILiquidTemplateEventHandler, SignalLiquidTemplateHandler>();
            services.AddLiquidFilter<SignalUrlFilter>("signal_url");
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "InvokeWorkflow",
                areaName: "OrchardCore.Workflows",
                template: "Workflows/{action}",
                defaults: new { controller = "Workflow" }
            );

            var workflowDefinitionStore = serviceProvider.GetRequiredService<IWorkflowTypeStore>();
            var workflowInstanceStore = serviceProvider.GetRequiredService<IWorkflowStore>();
            var workflowDefinitionDictionary = workflowDefinitionStore.ListAsync().GetAwaiter().GetResult().ToDictionary(x => x.WorkflowTypeId);
            var workflowInstanceDictionary = workflowInstanceStore.ListAsync().GetAwaiter().GetResult().ToDictionary(x => x.Id);

            ConfigureWorkflowRouteEntries(serviceProvider, workflowDefinitionDictionary, workflowInstanceDictionary);
        }

        private void ConfigureWorkflowRouteEntries(IServiceProvider serviceProvider, IDictionary<string, WorkflowType> workflowDefinitionDictionary, IDictionary<int, Workflow> workflowInstanceDictionary)
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
                from entry in WorkflowInstanceRouteEntries.GetWorkflowInstanceRoutesEntries(workflowDefinitionDictionary[workflowInstance.WorkflowTypeId], workflowInstance, activityLibrary)
                select entry;

            workflowDefinitionRouteEntries.AddEntries(workflowDefinitionRouteEntryQuery);
            workflowInstanceRouteEntries.AddEntries(workflowInstanceRouteEntryQuery);
        }
    }
}
