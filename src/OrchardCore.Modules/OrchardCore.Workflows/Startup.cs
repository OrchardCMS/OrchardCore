using Fluid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Deployment;
using OrchardCore.Workflows.Drivers;
using OrchardCore.Workflows.Evaluators;
using OrchardCore.Workflows.Events;
using OrchardCore.Workflows.Expressions;
using OrchardCore.Workflows.Handlers;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Recipes;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.WorkflowContextProviders;

namespace OrchardCore.Workflows;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
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
        services.AddDataMigration<Migrations>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<Permissions>();
        services.AddDisplayDriver<IActivity, MissingActivityDisplayDriver>();
        services.AddIndexProvider<WorkflowTypeIndexProvider>();
        services.AddIndexProvider<WorkflowIndexProvider>();
        services.AddScoped<IWorkflowExecutionContextHandler, DefaultWorkflowExecutionContextHandler>();
        services.AddScoped<IWorkflowExpressionEvaluator, LiquidWorkflowExpressionEvaluator>();
        services.AddScoped<IWorkflowScriptEvaluator, JavaScriptWorkflowScriptEvaluator>();

        services.AddScoped<IWorkflowFaultHandler, DefaultWorkflowFaultHandler>();
        services.AddActivity<WorkflowFaultEvent, WorkflowFaultEventDisplayDriver>();
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

        services.AddTrimmingServices(_shellConfiguration);
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<AllWorkflowTypeDeploymentSource, AllWorkflowTypeDeploymentStep, AllWorkflowTypeDeploymentStepDriver>();
    }
}

[Feature("OrchardCore.Workflows.Session")]
public sealed class SessionStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<CommitTransactionTask, CommitTransactionTaskDisplayDriver>();
    }
}
