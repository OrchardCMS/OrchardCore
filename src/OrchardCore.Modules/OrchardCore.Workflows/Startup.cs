using Fluid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.BackgroundTasks;
using OrchardCore.Admin;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Liquid;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Workflows.Options;
using OrchardCore.Workflows.ViewModels;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.BackgroundTasks;
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
            o.MemberAccessStrategy.Register<WorkflowFaultModel>();
            o.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Input", (obj, context) => new LiquidPropertyAccessor((LiquidTemplateContext)context, (name, context) => LiquidWorkflowExpressionEvaluator.ToFluidValue(obj.Input, name, context)));
            o.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Output", (obj, context) => new LiquidPropertyAccessor((LiquidTemplateContext)context, (name, context) => LiquidWorkflowExpressionEvaluator.ToFluidValue(obj.Output, name, context)));
            o.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Properties", (obj, context) => new LiquidPropertyAccessor((LiquidTemplateContext)context, (name, context) => LiquidWorkflowExpressionEvaluator.ToFluidValue(obj.Properties, name, context)));
            o.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Variables", (obj, context) => new LiquidPropertyAccessor((LiquidTemplateContext)context, (name, context) => LiquidWorkflowExpressionEvaluator.ToFluidValue(obj.Variables, name, context)));
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
        services.Configure<WorkflowJournalOptions>(_shellConfiguration.GetSection("Workflows:Journal"));
        services.AddScoped<IWorkflowExecutionJournal, WorkflowExecutionJournal>();
        // The real-time startup of this module, when SignalR is enabled, registers the notifier that sends the changes; it can
        // run before or after this one, so the default one doesn't replace it.
        services.TryAddScoped<IWorkflowDesignerNotifier, NullWorkflowDesignerNotifier>();
        services.AddScoped<IActivityPresetProvider, WorkflowActivityPresetProvider>();
        services.AddScoped<IWorkflowHandler, WorkflowJournalHandler>();
        services.AddIndexProvider<WorkflowExecutionRecordIndexProvider>();
        services.Configure<StoreCollectionOptions>(options => options.Collections.Add(WorkflowExecutionRecord.Collection));

        // Variable types; modules can add their own.
        services.AddWorkflowVariableType<StringVariableType>();
        services.AddWorkflowVariableType<NumberVariableType>();
        services.AddWorkflowVariableType<BooleanVariableType>();
        services.AddWorkflowVariableType<DateTimeVariableType>();
        services.AddWorkflowVariableType<ObjectVariableType>();
        services.AddWorkflowVariableType<ArrayVariableType>();
        services.AddWorkflowVariableType<AnyVariableType>();
        services.AddScoped<IWorkflowVariableTypeProvider, WorkflowVariableTypeProvider>();
        services.AddScoped<WorkflowVariableValidator>();
        services.AddScoped<IWorkflowStore, WorkflowStore>();
        services.AddScoped<IWorkflowManager, WorkflowManager>();
        services.AddScoped<IActivityDisplayManager, ActivityDisplayManager>();
        services.AddDataMigration<Migrations>();
        services.AddNavigationProvider<AdminMenu>();
        
        // Builds the rows of the workflow types admin list.
        services.AddDisplayDriver<WorkflowTypeEntry, WorkflowTypeEntryDisplayDriver>();
        services.AddDisplayDriver<WorkflowEntry, WorkflowEntryDisplayDriver>();
        services.AddPermissionProvider<Permissions>();
        services.AddDisplayDriver<IActivity, MissingActivityDisplayDriver>();
        services.AddDisplayDriver<IActivity, ActivityRetryPolicyDisplayDriver>();
        services.AddSingleton<IBackgroundTask, WorkflowRetryBackgroundTask>();
        services.AddIndexProvider<WorkflowTypeIndexProvider>();
        services.AddIndexProvider<WorkflowIndexProvider>();
        services.AddIndexProvider<WorkflowTypeDraftIndexProvider>();
        services.AddIndexProvider<WorkflowTypeVersionIndexProvider>();
        services.AddScoped<IWorkflowActivityEditorValidator, WorkflowActivityEditorValidator>();
        services.AddScoped<IWorkflowTypeDraftManager, WorkflowTypeDraftManager>();
        services.AddScoped<WorkflowDesignerModelBuilder>();
        services.AddScoped<IJSLocalizer, WorkflowsDesignerJSLocalizer>();
        services.AddScoped<IWorkflowTypeEventHandler, WorkflowTypeDraftHandler>();
        services.AddScoped<IWorkflowExecutionContextHandler, DefaultWorkflowExecutionContextHandler>();
        services.AddScoped<IWorkflowExpressionEvaluator, LiquidWorkflowExpressionEvaluator>();
        services.AddScoped<IWorkflowScriptEvaluator, JavaScriptWorkflowScriptEvaluator>();

        // Expression syntaxes; modules can add their own.
        services.AddWorkflowExpressionProvider<LiteralExpressionProvider>();
        services.AddWorkflowExpressionProvider<LiquidExpressionProvider>();
        services.AddWorkflowExpressionProvider<JavaScriptExpressionProvider>();
        services.AddScoped<IWorkflowExpressionManager, WorkflowExpressionManager>();
        services.AddScoped<WorkflowExpressionInputValidator>();

        services.AddScoped<IWorkflowFaultHandler, DefaultWorkflowFaultHandler>();

        // The default icons of the categories of this module's activities. Other modules set the icons of theirs.
        services.Configure<WorkflowOptions>(options =>
        {
            options.CategoryIcons["Background"] = "fa-solid fa-clock";
            options.CategoryIcons["Content"] = "fa-solid fa-file-lines";
            options.CategoryIcons["Control Flow"] = "fa-solid fa-diagram-project";
            options.CategoryIcons["Exceptions"] = "fa-solid fa-bug";
            options.CategoryIcons["HTTP"] = "fa-solid fa-globe";
            options.CategoryIcons["Primitives"] = "fa-solid fa-cube";
            options.CategoryIcons["Session"] = "fa-solid fa-database";
            options.CategoryIcons["UI"] = "fa-solid fa-window-maximize";
        });

        services.AddActivity<WorkflowFaultEvent, WorkflowFaultEventDisplayDriver>(activity => activity.Icon = "fa-solid fa-bug");
        services.AddActivity<StartedByWorkflowEvent, StartedByWorkflowEventDisplayDriver>(activity => activity.Icon = "fa-solid fa-right-to-bracket");
        services.AddActivity<Activity, ActivityMetadataDisplayDriver>();
        services.AddActivity<NotifyTask, NotifyTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-bell");
        services.AddActivity<SetPropertyTask, SetPropertyTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-pen-to-square");
        services.AddActivity<ExecuteWorkflowTask, ExecuteWorkflowTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-diagram-project");
        services.AddScoped<IWorkflowGlobalValueProvider, DefaultWorkflowGlobalValueProvider>();
        services.AddActivity<SetVariableTask, SetVariableTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-square-root-variable");
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
