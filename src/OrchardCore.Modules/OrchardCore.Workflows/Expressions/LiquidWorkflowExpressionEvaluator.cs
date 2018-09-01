using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
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
            context.MemberAccessStrategy.Register<LiquidPropertyAccessor, FluidValue>((obj, name) => obj.GetValueAsync(name));
            context.MemberAccessStrategy.Register<WorkflowExecutionContext>();
            context.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Input", obj => new LiquidPropertyAccessor(name => ToFluidValue(obj.Input, name)));
            context.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Output", obj => new LiquidPropertyAccessor(name => ToFluidValue(obj.Output, name)));
            context.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Properties", obj => new LiquidPropertyAccessor(name => ToFluidValue(obj.Properties, name)));
            context.SetValue("Workflow", workflowContext);

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

            // TODO: Extract the request culture.

            // Give modules a chance to add more things to the template context.
            foreach (var handler in services.GetServices<ILiquidTemplateEventHandler>())
            {
                await handler.RenderingAsync(context);
            }

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
