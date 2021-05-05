using System;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Entities;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Controllers;
using OrchardCore.Workflows.Deployment;
using OrchardCore.Workflows.Drivers;
using OrchardCore.Workflows.Evaluators;
using OrchardCore.Workflows.Expressions;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
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
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<WorkflowExecutionContext>();
                o.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Input", (obj, context) => new LiquidPropertyAccessor((LiquidTemplateContext)context, (name, context) => LiquidWorkflowExpressionEvaluator.ToFluidValue(obj.Input, name, context)));
                o.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Output", (obj, context) => new LiquidPropertyAccessor((LiquidTemplateContext)context, (name, context) => LiquidWorkflowExpressionEvaluator.ToFluidValue(obj.Output, name, context)));
                o.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Properties", (obj, context) => new LiquidPropertyAccessor((LiquidTemplateContext)context, (name, context) => LiquidWorkflowExpressionEvaluator.ToFluidValue(obj.Properties, name, context)));
            });

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
            services.AddScoped<IDisplayDriver<IActivity>, MissingActivityDisplayDriver>();
            services.AddSingleton<IIndexProvider, WorkflowTypeIndexProvider>();
            services.AddSingleton<IIndexProvider, WorkflowIndexProvider>();
            services.AddScoped<IWorkflowExecutionContextHandler, DefaultWorkflowExecutionContextHandler>();
            services.AddScoped<IWorkflowExpressionEvaluator, LiquidWorkflowExpressionEvaluator>();
            services.AddScoped<IWorkflowScriptEvaluator, JavaScriptWorkflowScriptEvaluator>();

            services.AddActivity<Activity, ActivityMetadataDisplayDriver>();
            services.AddActivity<NotifyTask, NotifyTaskDisplayDriver>();
            services.AddActivity<SetPropertyTask, SetVariableTaskDisplayDriver>();
            services.AddActivity<SetOutputTask, SetOutputTaskDisplayDriver>();
            services.AddActivity<CorrelateTask, CorrelateTaskDisplayDriver>();
            services.AddActivity<ForkTask, ForkTaskDisplayDriver>();
            services.AddActivity<JoinTask, JoinTaskDisplayDriver>();
            services.AddActivity<ForLoopTask, ForLoopTaskDisplayDriver>();
            services.AddActivity<ForEachTask, ForEachTaskDisplayDriver>();
            services.AddActivity<WhileLoopTask, WhileLoopTaskDisplayDriver>();
            services.AddActivity<IfElseTask, IfElseTaskDisplayDriver>();
            services.AddActivity<ScriptTask, ScriptTaskDisplayDriver>();
            services.AddActivity<LogTask, LogTaskDisplayDriver>();

            services.AddRecipeExecutionStep<WorkflowTypeStep>();
            services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
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

    [Feature("OrchardCore.Workflows.Session")]
    public class SessionStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddActivity<CommitTransactionTask, CommitTransactionTaskDisplayDriver>();
        }
    }
}
