using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Options;

/// <summary>
/// Holds the registered expression syntaxes. <c>services.AddWorkflowExpressionProvider&lt;TProvider&gt;()</c> adds
/// them.
/// </summary>
public sealed class WorkflowExpressionOptions
{
    private readonly List<WorkflowExpressionProviderDescriptor> _providers = [];

    /// <summary>
    /// Gets the registered syntaxes, in the order they were registered.
    /// </summary>
    public IReadOnlyList<WorkflowExpressionProviderDescriptor> Providers => _providers;

    /// <summary>
    /// Registers a syntax. It replaces a syntax registered earlier with the same name (ignoring case), so a module
    /// can replace a built-in syntax.
    /// </summary>
    /// <param name="descriptor">The syntax.</param>
    /// <returns>The options.</returns>
    public WorkflowExpressionOptions Add(WorkflowExpressionProviderDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        _providers.RemoveAll(provider => string.Equals(provider.Name, descriptor.Name, StringComparison.OrdinalIgnoreCase));
        _providers.Add(descriptor);

        return this;
    }
}
