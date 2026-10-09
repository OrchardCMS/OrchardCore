using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Expressions;

/// <summary>
/// Looks up the registered <see cref="IWorkflowExpressionProvider"/> services, and evaluates expressions with them.
/// </summary>
public sealed class WorkflowExpressionManager : IWorkflowExpressionManager
{
    private static readonly string[] s_order = [WorkflowExpressionSyntaxes.Literal, WorkflowExpressionSyntaxes.Liquid, WorkflowExpressionSyntaxes.JavaScript];

    private readonly Dictionary<string, IWorkflowExpressionProvider> _providers = new(StringComparer.OrdinalIgnoreCase);

    public WorkflowExpressionManager(IEnumerable<IWorkflowExpressionProvider> providers)
    {
        // The last registration of a name wins, so a module can replace a built-in syntax.
        foreach (var provider in providers)
        {
            _providers[provider.Name] = provider;
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<IWorkflowExpressionProvider> List()
        => _providers.Values
            .OrderBy(provider =>
            {
                var index = Array.FindIndex(s_order, name => string.Equals(name, provider.Name, StringComparison.OrdinalIgnoreCase));

                return index < 0 ? s_order.Length : index;
            })
            .ThenBy(provider => provider.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <inheritdoc />
    public IWorkflowExpressionProvider Get(string syntax)
        => !string.IsNullOrEmpty(syntax) && _providers.TryGetValue(syntax, out var provider) ? provider : null;

    /// <inheritdoc />
    public Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, string defaultSyntax = null, WorkflowExpressionEvaluationContext context = null)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var syntax = string.IsNullOrEmpty(expression.Syntax) ? defaultSyntax : expression.Syntax;
        var provider = Get(syntax) ?? throw new NotSupportedException($"The expression syntax '{syntax}' isn't available. Enable the feature that provides it.");

        return provider.EvaluateAsync(expression, workflowContext, context);
    }
}
