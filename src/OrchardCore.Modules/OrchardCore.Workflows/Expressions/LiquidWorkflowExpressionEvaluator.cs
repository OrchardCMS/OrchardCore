using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Expressions
{
    public class LiquidWorkflowExpressionEvaluator : IWorkflowExpressionEvaluator
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IEnumerable<IWorkflowExecutionContextHandler> _workflowContextHandlers;
        private readonly ILogger _logger;
        private readonly TemplateOptions _templateOptions;

        public LiquidWorkflowExpressionEvaluator(
            ILiquidTemplateManager liquidTemplateManager,
            IEnumerable<IWorkflowExecutionContextHandler> workflowContextHandlers,
            ILogger<LiquidWorkflowExpressionEvaluator> logger,
            IOptions<TemplateOptions> templateOptions
        )
        {
            _liquidTemplateManager = liquidTemplateManager;
            _workflowContextHandlers = workflowContextHandlers;
            _logger = logger;
            _templateOptions = templateOptions.Value;
        }

        public async Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, TextEncoder encoder)
        {
            var templateContext = new TemplateContext(_templateOptions);
            var expressionContext = new WorkflowExecutionExpressionContext(templateContext, workflowContext);

            await _workflowContextHandlers.InvokeAsync((h, expressionContext) => h.EvaluatingExpressionAsync(expressionContext), expressionContext, _logger);

            // Set WorkflowContext as a local scope property.
            var result = await _liquidTemplateManager.RenderStringAsync(
                expression.Expression,
                encoder ?? NullEncoder.Default,
                new Dictionary<string, FluidValue>() { ["Workflow"] = new ObjectValue(workflowContext) }
                );

            return String.IsNullOrWhiteSpace(result) ? default : (T)Convert.ChangeType(result, typeof(T));
        }

        public static Task<FluidValue> ToFluidValue(IDictionary<string, object> dictionary, string key, TemplateContext context)
        {
            if (!dictionary.ContainsKey(key))
            {
                return Task.FromResult<FluidValue>(NilValue.Instance);
            }

            return Task.FromResult(FluidValue.Create(dictionary[key], context.Options));
        }
    }
}
