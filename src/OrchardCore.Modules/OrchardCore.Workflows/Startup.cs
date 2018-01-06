using System;
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
using YesSql;
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
            services.AddSingleton<IIndexProvider, HttpRequestEventIndexProvider>();
            services.AddSingleton<IIndexProvider, HttpRequestFilterEventIndexProvider>();

            services.AddScoped<IActivity, NotifyTask>();
            services.AddScoped<IActivity, SetVariableTask>();
            services.AddScoped<IActivity, SetOutputTask>();
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

            services.AddSingleton<IWorkflowRouteEntries, WorkflowRouteEntries>();
            services.AddSingleton<IWorkflowPathEntries, WorkflowPathEntries>();
            services.AddSingleton<IGlobalMethodProvider, HttpContextMethodProvider>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var routeEntries = serviceProvider.GetRequiredService<IWorkflowRouteEntries>();
            var pathEntries = serviceProvider.GetRequiredService<IWorkflowPathEntries>();
            var session = serviceProvider.GetRequiredService<ISession>();
            var workflowRoutes = session.QueryIndex<WorkflowDefinitionByHttpRequestFilterIndex>().ListAsync().GetAwaiter().GetResult().GroupBy(x => x.WorkflowDefinitionId);
            var workflowPaths = session.QueryIndex<WorkflowDefinitionByHttpRequestIndex>().ListAsync().GetAwaiter().GetResult().GroupBy(x => x.WorkflowDefinitionId);

            foreach (var item in workflowRoutes)
            {
                routeEntries.AddEntries(item.Key, item.Select(x => new WorkflowRoutesEntry
                {
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    ActivityId = x.ActivityId,
                    HttpMethod = x.HttpMethod,
                    RouteValues = new RouteValueDictionary
                {
                    { "controller", x.ControllerName },
                    { "action", x.ActionName },
                    { "area", x.AreaName }
                }
                }));
            }

            foreach (var item in workflowPaths)
            {
                pathEntries.AddEntries(item.Key, item.Select(x => new WorkflowPathEntry
                {
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    ActivityId = x.ActivityId,
                    HttpMethod = x.HttpMethod,
                    Path = x.RequestPath
                }));
            }

            var workflowRoute = new WorkflowRouter(routes.DefaultHandler);

            routes.Routes.Insert(0, workflowRoute);
        }
    }
}
