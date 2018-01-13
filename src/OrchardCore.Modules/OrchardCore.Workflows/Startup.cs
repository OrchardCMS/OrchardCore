using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Environment.Navigation;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Drivers;
using OrchardCore.Workflows.Evaluators;
using OrchardCore.Workflows.Expressions;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Liquid;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.WorkflowContextProviders;
using YesSql.Indexes;

namespace OrchardCore.Workflows
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection();
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
            services.AddScoped<IDisplayDriver<IActivity>, MissingActivityDisplay>();
            services.AddSingleton<IIndexProvider, WorkflowDefinitionIndexProvider>();
            services.AddSingleton<IIndexProvider, WorkflowInstanceIndexProvider>();
            services.AddScoped<IWorkflowContextHandler, DefaultWorkflowContextHandler>();
            services.AddScoped<IWorkflowContextHandler, SignalWorkflowContextHandler>();
            services.AddScoped<IWorkflowExpressionEvaluator, LiquidWorkflowExpressionEvaluator>();
            services.AddScoped<IWorkflowScriptEvaluator, DefaultWorkflowScriptEvaluator>();

            services.AddActivity<NotifyTask, NotifyTaskDisplay>();
            services.AddActivity<SetPropertyTask, SetVariableTaskDisplay>();
            services.AddActivity<SetOutputTask, SetOutputTaskDisplay>();
            services.AddActivity<CorrelateTask, CorrelateTaskDisplay>();
            services.AddActivity<BranchTask, BranchTaskDisplay>();
            services.AddActivity<ForLoopTask, ForLoopTaskDisplay>();
            services.AddActivity<WhileLoopTask, WhileLoopTaskDisplay>();
            services.AddActivity<IfElseTask, IfElseTaskDisplay>();
            services.AddActivity<ScriptTask, ScriptTaskDisplay>();
            services.AddActivity<SignalEvent, SignalEventDisplay>();

            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<ILiquidTemplateEventHandler, SignalLiquidTemplateHandler>();
            services.AddLiquidFilter<SignalUrlFilter>("signal_url");
        }
    }
}
