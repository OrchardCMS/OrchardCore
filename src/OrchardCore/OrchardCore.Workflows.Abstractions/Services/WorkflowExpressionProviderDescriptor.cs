using OrchardCore.Localization;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Describes a registered expression syntax with the static members of its class, so the syntaxes can be listed
/// without creating their providers. <c>services.AddWorkflowExpressionProvider&lt;TProvider&gt;()</c> registers one
/// per syntax.
/// </summary>
public sealed class WorkflowExpressionProviderDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowExpressionProviderDescriptor"/> class.
    /// </summary>
    /// <param name="name">The name of the syntax, stored in <see cref="Models.WorkflowExpression{T}.Syntax"/>.</param>
    /// <param name="displayName">The name shown in the expression editors.</param>
    /// <param name="editorLanguage">The Monaco language of the multi-line editor.</param>
    /// <param name="type">The class of the provider, resolved from the services when an expression is evaluated.</param>
    public WorkflowExpressionProviderDescriptor(string name, LocalizationSource displayName, string editorLanguage, Type type)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(type);

        Name = name;
        DisplayName = displayName;
        EditorLanguage = editorLanguage;
        Type = type;
    }

    /// <summary>
    /// Gets the name of the syntax, stored in <see cref="Models.WorkflowExpression{T}.Syntax"/>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the name shown in the expression editors, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    public LocalizationSource DisplayName { get; }

    /// <summary>
    /// Gets the Monaco language of the multi-line editor, such as <c>plaintext</c>, <c>liquid</c> or
    /// <c>javascript</c>.
    /// </summary>
    public string EditorLanguage { get; }

    /// <summary>
    /// Gets the class of the provider, which implements <see cref="IWorkflowExpressionProvider"/>.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Describes an expression syntax from the static members of its provider's class.
    /// </summary>
    /// <typeparam name="TProvider">The class of the provider.</typeparam>
    /// <returns>The descriptor.</returns>
    public static WorkflowExpressionProviderDescriptor Create<TProvider>()
        where TProvider : class, IWorkflowExpressionProvider
    {
        return new WorkflowExpressionProviderDescriptor(TProvider.Name, TProvider.DisplayName, TProvider.EditorLanguage, typeof(TProvider));
    }
}
