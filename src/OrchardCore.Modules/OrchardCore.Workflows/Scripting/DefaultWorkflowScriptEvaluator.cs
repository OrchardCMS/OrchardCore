using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Evaluators
{
    public class DefaultWorkflowScriptEvaluator : IWorkflowScriptEvaluator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IScriptingManager _scriptingManager;
        private readonly IEnumerable<IWorkflowContextHandler> _workflowContextHandlers;
        private readonly ILogger<DefaultWorkflowScriptEvaluator> _logger;

        public DefaultWorkflowScriptEvaluator(
            IServiceProvider serviceProvider,
            IScriptingManager scriptingManager,
            IEnumerable<IWorkflowContextHandler> workflowContextHandlers,
            IStringLocalizer<DefaultWorkflowScriptEvaluator> localizer,
            ILogger<DefaultWorkflowScriptEvaluator> logger
        )
        {
            _serviceProvider = serviceProvider;
            _scriptingManager = scriptingManager;
            _workflowContextHandlers = workflowContextHandlers;
            _logger = logger;
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public async Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowContext workflowContext, params IGlobalMethodProvider[] scopedMethodProviders)
        {
            var workflowDefinition = workflowContext.WorkflowDefinition;
            var prefix = workflowDefinition.ScriptingEngine;
            var directive = $"{prefix}:{expression}";
            var expressionContext = new WorkflowContextScriptContext(workflowContext);

            await _workflowContextHandlers.InvokeAsync(async x => await x.EvaluatingScriptAsync(expressionContext), _logger);

            var methodProviders = scopedMethodProviders.Concat(expressionContext.ScopedMethodProviders);
            return (T)_scriptingManager.Evaluate(directive, methodProviders);
        }
    }
}
