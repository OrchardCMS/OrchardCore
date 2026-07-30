using System.Collections;
using System.Globalization;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;
using Fluid.Values;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Expressions;

public class LiquidWorkflowExpressionEvaluator : IWorkflowExpressionEvaluator
{
    private readonly LiquidViewParser _liquidViewParser;
    private readonly IEnumerable<IWorkflowExecutionContextHandler> _workflowContextHandlers;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly TemplateOptions _templateOptions;

    public LiquidWorkflowExpressionEvaluator(
        LiquidViewParser liquidViewParser,
        IEnumerable<IWorkflowExecutionContextHandler> workflowContextHandlers,
        IServiceProvider serviceProvider,
        ILogger<LiquidWorkflowExpressionEvaluator> logger,
        IOptions<TemplateOptions> templateOptions
    )
    {
        _liquidViewParser = liquidViewParser;
        _workflowContextHandlers = workflowContextHandlers;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _templateOptions = templateOptions.Value;
    }

    public async Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, TextEncoder encoder)
    {
        if (string.IsNullOrWhiteSpace(expression.Expression))
        {
            return default;
        }

        var templateContext = new LiquidTemplateContext(_serviceProvider, _templateOptions);
        var expressionContext = new WorkflowExecutionExpressionContext(templateContext, workflowContext);

        await _workflowContextHandlers.InvokeAsync((h, expressionContext) => h.EvaluatingExpressionAsync(expressionContext), expressionContext, _logger);

        templateContext.SetValue("Workflow", new ObjectValue(workflowContext));
        var template = GetTemplate(expression.Expression);

        if (typeof(T) != typeof(string) && TryGetSingleOutputStatement(template, out var outputStatement))
        {
            var fluidValue = await EvaluateOutputValueAsync(outputStatement, templateContext);
            return ConvertValue<T>(fluidValue.ToObjectValue(), fluidValue.ToStringValue());
        }

        var result = await RenderTemplateAsync(template, templateContext, encoder ?? NullEncoder.Default);

        return ConvertValue<T>(result, result);
    }

    private IFluidTemplate GetTemplate(string expression)
    {
        if (!_liquidViewParser.TryParse(expression, out var template, out var error))
        {
            _liquidViewParser.TryParse(error, out template, out _);
        }

        return template;
    }

    private static bool TryGetSingleOutputStatement(IFluidTemplate template, out OutputStatement outputStatement)
    {
        if (template is IStatementList { Statements.Count: 1 } statementList &&
            statementList.Statements[0] is OutputStatement output)
        {
            outputStatement = output;
            return true;
        }

        outputStatement = null;
        return false;
    }

    private static ValueTask<FluidValue> EvaluateOutputValueAsync(OutputStatement outputStatement, TemplateContext templateContext)
        => outputStatement.Expression.EvaluateAsync(templateContext);

    private static async Task<string> RenderTemplateAsync(IFluidTemplate template, TemplateContext templateContext, TextEncoder encoder)
    {
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        await template.RenderAsync(writer, encoder, templateContext);

        return writer.ToString();
    }

    private static T ConvertValue<T>(object value, string stringValue)
    {
        if (value == null || string.IsNullOrWhiteSpace(stringValue) && value is string)
        {
            return default;
        }

        if (value is T typedValue)
        {
            return typedValue;
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)stringValue;
        }

        if (typeof(T) == typeof(IEnumerable<object>) && value is IEnumerable enumerable and not string)
        {
            return (T)(object)enumerable.Cast<object>();
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }
        catch (InvalidCastException)
        {
            return (T)Convert.ChangeType(stringValue, typeof(T), CultureInfo.InvariantCulture);
        }
    }

    public static Task<FluidValue> ToFluidValue(IDictionary<string, object> dictionary, string key, TemplateContext context)
    {
        if (!dictionary.TryGetValue(key, out var value))
        {
            return Task.FromResult<FluidValue>(NilValue.Instance);
        }

        return Task.FromResult(FluidValue.Create(value, context.Options));
    }
}
