using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Options;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Expressions;

/// <summary>
/// The default <see cref="IWorkflowExpressionManager"/>, built from the syntaxes of
/// <see cref="WorkflowExpressionOptions"/>. It lists them from their descriptors, and resolves a provider from the
/// services of the scope only when it is used.
/// </summary>
public sealed class WorkflowExpressionManager : IWorkflowExpressionManager
{
    private static readonly string[] s_order = [WorkflowExpressionSyntaxes.Literal, WorkflowExpressionSyntaxes.Liquid, WorkflowExpressionSyntaxes.JavaScript];

    private readonly IServiceProvider _serviceProvider;
    private readonly IReadOnlyList<WorkflowExpressionProviderDescriptor> _providers;
    private readonly Dictionary<string, WorkflowExpressionProviderDescriptor> _providersByName = new(StringComparer.OrdinalIgnoreCase);

    public WorkflowExpressionManager(IOptions<WorkflowExpressionOptions> options, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(options);

        _serviceProvider = serviceProvider;

        // The options keep the last registration of a name, so a module can replace a built-in syntax.
        foreach (var provider in options.Value.Providers)
        {
            _providersByName[provider.Name] = provider;
        }

        _providers = _providersByName.Values
            .OrderBy(provider =>
            {
                var index = Array.FindIndex(s_order, name => string.Equals(name, provider.Name, StringComparison.OrdinalIgnoreCase));

                return index < 0 ? s_order.Length : index;
            })
            .ThenBy(provider => provider.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<WorkflowExpressionProviderDescriptor> List() => _providers;

    /// <inheritdoc />
    public WorkflowExpressionProviderDescriptor GetDescriptor(string syntax)
    {
        return string.IsNullOrEmpty(syntax) ? null : _providersByName.GetValueOrDefault(syntax);
    }

    /// <inheritdoc />
    public IWorkflowExpressionProvider Get(string syntax)
    {
        return GetDescriptor(syntax) is { } descriptor
            ? (IWorkflowExpressionProvider)_serviceProvider.GetRequiredService(descriptor.Type)
            : null;
    }

    /// <inheritdoc />
    public Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, string defaultSyntax = null, WorkflowExpressionEvaluationContext context = null)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var syntax = string.IsNullOrEmpty(expression.Syntax) ? defaultSyntax : expression.Syntax;
        var provider = Get(syntax) ?? throw new NotSupportedException($"The expression syntax '{syntax}' isn't available. Enable the feature that provides it.");

        return provider.EvaluateAsync(expression, workflowContext, context);
    }
}
