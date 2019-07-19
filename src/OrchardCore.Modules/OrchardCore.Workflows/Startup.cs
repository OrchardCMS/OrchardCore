using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Deployment;
using OrchardCore.Workflows.Drivers;
using OrchardCore.Workflows.Evaluators;
using OrchardCore.Workflows.Expressions;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Recipes;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.WorkflowContextProviders;
using YesSql.Indexes;

namespace OrchardCore.Workflows
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddIdGeneration();
            services.AddSingleton<IWorkflowTypeIdGenerator, WorkflowTypeIdGenerator>();
            services.AddSingleton<IWorkflowIdGenerator, WorkflowIdGenerator>();
            services.AddSingleton<IActivityIdGenerator, ActivityIdGenerator>();

            services.AddScoped(typeof(Resolver<>));
            services.AddSingleton<ISecurityTokenService, SecurityTokenService>();
            services.AddScoped<IActivityLibrary, ActivityLibrary>();
            services.AddScoped<IWorkflowTypeStore, WorkflowTypeStore>();
            services.AddScoped<IWorkflowStore, WorkflowStore>();
            services.AddScoped<IWorkflowManager, WorkflowManager>();
            services.AddScoped<IActivityDisplayManager, ActivityDisplayManager>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IDisplayDriver<IActivity>, MissingActivityDisplay>();
            services.AddSingleton<IIndexProvider, WorkflowTypeIndexProvider>();
            services.AddSingleton<IIndexProvider, WorkflowIndexProvider>();
            services.AddScoped<IWorkflowExecutionContextHandler, DefaultWorkflowExecutionContextHandler>();
            services.AddScoped<IWorkflowExpressionEvaluator, LiquidWorkflowExpressionEvaluator>();
            services.AddScoped<IWorkflowScriptEvaluator, JavaScriptWorkflowScriptEvaluator>();

            services.AddActivity<Activity, ActivityMetadataDisplay>();
            services.AddActivity<NotifyTask, NotifyTaskDisplay>();
            services.AddActivity<SetPropertyTask, SetVariableTaskDisplay>();
            services.AddActivity<SetOutputTask, SetOutputTaskDisplay>();
            services.AddActivity<CorrelateTask, CorrelateTaskDisplay>();
            services.AddActivity<ForkTask, ForkTaskDisplay>();
            services.AddActivity<JoinTask, JoinTaskDisplay>();
            services.AddActivity<ForLoopTask, ForLoopTaskDisplay>();
            services.AddActivity<ForEachTask, ForEachTaskDisplay>();
            services.AddActivity<WhileLoopTask, WhileLoopTaskDisplay>();
            services.AddActivity<IfElseTask, IfElseTaskDisplay>();
            services.AddActivity<ScriptTask, ScriptTaskDisplay>();
            services.AddActivity<LogTask, LogTaskDisplay>();

            services.AddRecipeExecutionStep<WorkflowTypeStep>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "AddActivity",
                areaName: "OrchardCore.Workflows",
                template: "Admin/Workflows/Types/{workflowTypeId}/Activity/{activityName}/Add",
                defaults: new { controller = "Activity", action = "Create" }
            );

            routes.MapAreaRoute(
                name: "EditActivity",
                areaName: "OrchardCore.Workflows",
                template: "Admin/Workflows/Types/{workflowTypeId}/Activity/{activityId}/Edit",
                defaults: new { controller = "Activity", action = "Edit" }
            );

            routes.MapAreaRoute(
                name: "Workflows",
                areaName: "OrchardCore.Workflows",
                template: "Admin/Workflows/Types/{workflowTypeId}/Instances/{action}",
                defaults: new { controller = "Workflow", action = "Index" }
            );

            routes.MapAreaRoute(
                name: "WorkflowTypes",
                areaName: "OrchardCore.Workflows",
                template: "Admin/Workflows/Types/{action}/{id?}",
                defaults: new { controller = "WorkflowType", action = "Index" }
            );
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, AllWorkflowTypeDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllWorkflowTypeDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AllWorkflowTypeDeploymentStepDriver>();
        }
    }
}
