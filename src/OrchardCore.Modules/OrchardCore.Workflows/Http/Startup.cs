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

            services.AddScoped<IWorkflowTypeEventHandler, WorkflowTypeRoutesHandler>();
            services.AddScoped<IWorkflowHandler, WorkflowRoutesHandler>();

            services.AddSingleton<IWorkflowTypeRouteEntries, WorkflowTypeRouteEntries>();
            services.AddSingleton<IWorkflowInstanceRouteEntries, WorkflowRouteEntries>();
            services.AddSingleton<IGlobalMethodProvider, HttpMethodsProvider>();
            services.AddScoped<IWorkflowExecutionContextHandler, SignalWorkflowExecutionContextHandler>();

            services.AddActivity<HttpRequestEvent, HttpRequestEventDisplay>();
            services.AddActivity<HttpRequestFilterEvent, HttpRequestFilterEventDisplay>();
            services.AddActivity<HttpRedirectTask, HttpRedirectTaskDisplay>();
            services.AddActivity<HttpRequestTask, HttpRequestTaskDisplay>();
            services.AddActivity<HttpResponseTask, HttpResponseTaskDisplay>();
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
                defaults: new { controller = "HttpWorkflow" }
            );

            var workflowTypeStore = serviceProvider.GetRequiredService<IWorkflowTypeStore>();
            var workflowStore = serviceProvider.GetRequiredService<IWorkflowStore>();
            var workflowTypeDictionary = workflowTypeStore.ListAsync().GetAwaiter().GetResult().ToDictionary(x => x.WorkflowTypeId);
            var workflowDictionary = workflowStore.ListAsync().GetAwaiter().GetResult().ToDictionary(x => x.Id);

            ConfigureWorkflowRouteEntries(serviceProvider, workflowTypeDictionary, workflowDictionary);
        }

        private void ConfigureWorkflowRouteEntries(IServiceProvider serviceProvider, IDictionary<string, WorkflowType> workflowTypeDictionary, IDictionary<int, Workflow> workflowDictionary)
        {
            var activityLibrary = serviceProvider.GetRequiredService<IActivityLibrary>();
            var workflowTypeRouteEntries = serviceProvider.GetRequiredService<IWorkflowTypeRouteEntries>();
            var workflowEntries = serviceProvider.GetRequiredService<IWorkflowInstanceRouteEntries>();

            var workflowTypeRouteEntryQuery =
                from workflowType in workflowTypeDictionary.Values
                from entry in WorkflowTypeRouteEntries.GetWorkflowTypeRoutesEntries(workflowType, activityLibrary)
                select entry;

            var workflowRouteEntryQuery =
                from workflow in workflowDictionary.Values
                from entry in WorkflowRouteEntries.GetWorkflowRoutesEntries(workflowTypeDictionary[workflow.WorkflowTypeId], workflow, activityLibrary)
                select entry;

            workflowTypeRouteEntries.AddEntries(workflowTypeRouteEntryQuery);
            workflowEntries.AddEntries(workflowRouteEntryQuery);
        }
    }
}
