using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Controllers;
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
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

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

            services.AddActivity<CommitTransactionTask, CommitTransactionTaskDisplay>();

            services.AddRecipeExecutionStep<WorkflowTypeStep>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var activityControllerName = typeof(ActivityController).ControllerName();
            routes.MapAreaControllerRoute(
                name: "AddActivity",
                areaName: "OrchardCore.Workflows",
                pattern: _adminOptions.AdminUrlPrefix + "/Workflows/Types/{workflowTypeId}/Activity/{activityName}/Add",
                defaults: new { controller = activityControllerName, action = nameof(ActivityController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "EditActivity",
                areaName: "OrchardCore.Workflows",
                pattern: _adminOptions.AdminUrlPrefix + "/Workflows/Types/{workflowTypeId}/Activity/{activityId}/Edit",
                defaults: new { controller = activityControllerName, action = nameof(ActivityController.Edit) }
            );

            var workflowControllerName = typeof(WorkflowController).ControllerName();
            routes.MapAreaControllerRoute(
                name: "Workflows",
                areaName: "OrchardCore.Workflows",
                pattern: _adminOptions.AdminUrlPrefix + "/Workflows/Types/{workflowTypeId}/Instances/{action}",
                defaults: new { controller = workflowControllerName, action = nameof(WorkflowController.Index) }
            );

            var workflowTypeControllerName = typeof(WorkflowTypeController).ControllerName();
            routes.MapAreaControllerRoute(
                name: "WorkflowTypes",
                areaName: "OrchardCore.Workflows",
                pattern: _adminOptions.AdminUrlPrefix + "/Workflows/Types/{action}/{id?}",
                defaults: new { controller = workflowTypeControllerName, action = nameof(WorkflowTypeController.Index) }
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
