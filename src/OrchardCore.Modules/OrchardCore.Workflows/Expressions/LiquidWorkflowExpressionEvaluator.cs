using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Liquid;
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
        private readonly ILogger<LiquidWorkflowExpressionEvaluator> _logger;

        public LiquidWorkflowExpressionEvaluator(
            ILiquidTemplateManager liquidTemplateManager,
            IStringLocalizer<LiquidWorkflowExpressionEvaluator> localizer,
            IEnumerable<IWorkflowExecutionContextHandler> workflowContextHandlers,
            ILogger<LiquidWorkflowExpressionEvaluator> logger
        )
        {
            _liquidTemplateManager = liquidTemplateManager;
            _workflowContextHandlers = workflowContextHandlers;
            _logger = logger;
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public async Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext)
        {
            var templateContext = CreateTemplateContext(workflowContext);
            var expressionContext = new WorkflowExecutionExpressionContext(templateContext, workflowContext);

            await _workflowContextHandlers.InvokeAsync(x => x.EvaluatingExpressionAsync(expressionContext), _logger);

            var result = await _liquidTemplateManager.RenderAsync(expression.Expression, System.Text.Encodings.Web.JavaScriptEncoder.Default, templateContext);
            return string.IsNullOrWhiteSpace(result) ? default(T) : (T)Convert.ChangeType(result, typeof(T));
        }

        private TemplateContext CreateTemplateContext(WorkflowExecutionContext workflowContext)
        {
            var context = new TemplateContext();

            // Set WorkflowContext as the model.
            context.MemberAccessStrategy.Register<LiquidPropertyAccessor, FluidValue>((obj, name) => obj.GetValueAsync(name));
            context.MemberAccessStrategy.Register<WorkflowExecutionContext>();
            context.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Input", obj => new LiquidPropertyAccessor(name => ToFluidValue(obj.Input, name)));
            context.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Output", obj => new LiquidPropertyAccessor(name => ToFluidValue(obj.Output, name)));
            context.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Properties", obj => new LiquidPropertyAccessor(name => ToFluidValue(obj.Properties, name)));

            context.SetValue("Workflow", workflowContext);
            return context;
        }

        private Task<FluidValue> ToFluidValue(IDictionary<string, object> dictionary, string key)
        {
            if (!dictionary.ContainsKey(key))
                return Task.FromResult(default(FluidValue));

            return Task.FromResult(FluidValue.Create(dictionary[key]));
        }
    }
}
