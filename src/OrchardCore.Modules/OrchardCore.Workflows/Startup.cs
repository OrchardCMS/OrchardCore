using System;
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
using OrchardCore.Workflows.Indexes;
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
            services.AddSingleton<IIndexProvider, HttpRequestEventWorkflowDefinitionIndexProvider>();

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

            services.AddScoped<IWorkflowContextProvider, DefaultWorkflowContextProvider>();
            services.AddScoped<IWorkflowContextProvider, SignalWorkflowContextProvider>();

            services.AddSingleton<IGlobalMethodProvider, HttpContextMethodProvider>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var workflowRoute = new WorkflowRouter(routes.DefaultHandler);

            routes.Routes.Insert(0, workflowRoute);
        }
    }
}
