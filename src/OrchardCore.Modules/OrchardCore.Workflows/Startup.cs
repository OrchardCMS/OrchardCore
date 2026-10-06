using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Liquid;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
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
using OrchardCore.Workflows.Variables;
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
        services.AddScoped<IWorkflowTypeVersionStore, WorkflowTypeVersionStore>();
        services.Configure<WorkflowVersionOptions>(_shellConfiguration.GetSection("Workflows:Versions"));

        // Variable types; modules can add their own.
        services.AddScoped<IWorkflowVariableType, StringVariableType>();
        services.AddScoped<IWorkflowVariableType, NumberVariableType>();
        services.AddScoped<IWorkflowVariableType, BooleanVariableType>();
        services.AddScoped<IWorkflowVariableType, DateTimeVariableType>();
        services.AddScoped<IWorkflowVariableType, ObjectVariableType>();
        services.AddScoped<IWorkflowVariableType, ArrayVariableType>();
        services.AddScoped<IWorkflowVariableType, AnyVariableType>();
        services.AddScoped<IWorkflowVariableTypeProvider, WorkflowVariableTypeProvider>();
        services.AddScoped<WorkflowVariableValidator>();
        services.AddScoped<IWorkflowStore, WorkflowStore>();
        services.AddScoped<IWorkflowManager, WorkflowManager>();
        services.AddScoped<IActivityDisplayManager, ActivityDisplayManager>();
        services.AddDataMigration<Migrations>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<Permissions>();
        services.AddDisplayDriver<IActivity, MissingActivityDisplayDriver>();
        services.AddIndexProvider<WorkflowTypeIndexProvider>();
        services.AddIndexProvider<WorkflowIndexProvider>();
        services.AddIndexProvider<WorkflowTypeDraftIndexProvider>();
        services.AddIndexProvider<WorkflowTypeVersionIndexProvider>();
        services.AddScoped<IWorkflowTypeDraftManager, WorkflowTypeDraftManager>();
        services.AddScoped<WorkflowDesignerModelBuilder>();
        services.AddScoped<IJSLocalizer, WorkflowsDesignerJSLocalizer>();
        services.AddScoped<IWorkflowTypeEventHandler, WorkflowTypeDraftHandler>();
        services.AddScoped<IWorkflowExecutionContextHandler, DefaultWorkflowExecutionContextHandler>();
        services.AddScoped<IWorkflowExpressionEvaluator, LiquidWorkflowExpressionEvaluator>();
        services.AddScoped<IWorkflowScriptEvaluator, JavaScriptWorkflowScriptEvaluator>();

        services.AddScoped<IWorkflowFaultHandler, DefaultWorkflowFaultHandler>();
        services.AddActivity<WorkflowFaultEvent, WorkflowFaultEventDisplayDriver>(activity => activity.Icon = "fa-solid fa-bug");
        services.AddActivity<Activity, ActivityMetadataDisplayDriver>();
        services.AddActivity<NotifyTask, NotifyTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-bell");
        services.AddActivity<SetPropertyTask, SetVariableTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-pen-to-square");
        services.AddActivity<SetOutputTask, SetOutputTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-right-from-bracket");
        services.AddActivity<CorrelateTask, CorrelateTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-link");
        services.AddActivity<ForkTask, ForkTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-code-fork");
        services.AddActivity<JoinTask, JoinTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-code-merge");
        services.AddActivity<ForLoopTask, ForLoopTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-repeat");
        services.AddActivity<ForEachTask, ForEachTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-list-ol");
        services.AddActivity<WhileLoopTask, WhileLoopTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-rotate");
        services.AddActivity<IfElseTask, IfElseTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-code-branch");
        services.AddActivity<ScriptTask, ScriptTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-code");
        services.AddActivity<LiquidTask, LiquidTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-droplet");
        services.AddActivity<LogTask, LogTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-file-lines");

        services.AddRecipeExecutionStep<WorkflowTypeStep>();
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();

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
        services.AddActivity<CommitTransactionTask, CommitTransactionTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-database");
    }
}

[RequireFeatures("OrchardCore.Liquid")]
public sealed class LiquidStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IWorkflowExecutionContextHandler, LiquidViewTemplateWorkflowExecutionContextHandler>();
    }
}
