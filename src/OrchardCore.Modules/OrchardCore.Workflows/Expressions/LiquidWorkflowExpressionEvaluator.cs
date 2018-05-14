using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Expressions
{
    public class LiquidWorkflowExpressionEvaluator : IWorkflowExpressionEvaluator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IEnumerable<IWorkflowExecutionContextHandler> _workflowContextHandlers;
        private readonly ILogger<LiquidWorkflowExpressionEvaluator> _logger;

        public LiquidWorkflowExpressionEvaluator(
            IServiceProvider serviceProvider,
            ILiquidTemplateManager liquidTemplateManager,
            IStringLocalizer<LiquidWorkflowExpressionEvaluator> localizer,
            IEnumerable<IWorkflowExecutionContextHandler> workflowContextHandlers,
            ILogger<LiquidWorkflowExpressionEvaluator> logger
        )
        {
            _serviceProvider = serviceProvider;
            _liquidTemplateManager = liquidTemplateManager;
            _workflowContextHandlers = workflowContextHandlers;
            _logger = logger;
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public async Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext)
        {
            var templateContext = await CreateTemplateContextAsync(workflowContext);
            var expressionContext = new WorkflowExecutionExpressionContext(templateContext, workflowContext);

            await _workflowContextHandlers.InvokeAsync(async x => await x.EvaluatingExpressionAsync(expressionContext), _logger);

            var result = await _liquidTemplateManager.RenderAsync(expression.Expression, templateContext);
            return string.IsNullOrWhiteSpace(result) ? default(T) : (T)Convert.ChangeType(result, typeof(T));
        }

        private async Task<TemplateContext> CreateTemplateContextAsync(WorkflowExecutionContext workflowContext)
        {
            var context = new TemplateContext();
            var services = _serviceProvider;

            // Set WorkflowContext as the model.
            context.MemberAccessStrategy.Register<WorkflowExecutionContext>();
            context.SetValue(nameof(WorkflowExecutionContext), workflowContext);
            context.SetValue("CorrelationId", workflowContext.CorrelationId);

            // TODO: Add Liquid filters to easily access values from Input and Properties.
            context.SetValue("Input", workflowContext.Input);
            context.SetValue("Properties", workflowContext.Properties);

            // TODO: For now, simply add each Input and Property to the context, with the risk of overwriting items with the same key.
            // Add workflow input.
            foreach (var item in workflowContext.Input)
            {
                context.SetValue(item.Key, item.Value);
            }

            // Add workflow properties.
            foreach (var item in workflowContext.Properties)
            {
                context.SetValue(item.Key, item.Value);
            }

            // Add LastResult.
            context.SetValue("LastResult", workflowContext.LastResult);

            // Add services.
            context.AmbientValues.Add("Services", services);

            // Add UrlHelper, if we have an MVC Action context.
            var actionContext = services.GetService<IActionContextAccessor>()?.ActionContext;
            if (actionContext != null)
            {
                var urlHelperFactory = services.GetRequiredService<IUrlHelperFactory>();
                var urlHelper = urlHelperFactory.GetUrlHelper(actionContext);
                context.AmbientValues.Add("UrlHelper", urlHelper);
            }

            // Add ShapeFactory.
            var shapeFactory = services.GetRequiredService<IShapeFactory>();
            context.AmbientValues.Add("ShapeFactory", shapeFactory);

            // Add View Localizer.
            var localizer = services.GetRequiredService<IViewLocalizer>();
            context.AmbientValues.Add("ViewLocalizer", localizer);

            // TODO: Extract the request culture

            // Give modules a chance to add more things to the template context.
            foreach (var handler in services.GetServices<ILiquidTemplateEventHandler>())
            {
                await handler.RenderingAsync(context);
            }

            return context;
        }
    }
}
