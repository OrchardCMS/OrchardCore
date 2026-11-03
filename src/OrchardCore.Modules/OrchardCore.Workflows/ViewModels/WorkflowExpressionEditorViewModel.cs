using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

/// <summary>
/// The model of the <c>WorkflowExpressionEditor</c> shape: an expression with a select of its syntax. It posts
/// <c>{Name}.Expression</c> and <c>{Name}.Syntax</c>, which bind to a <see cref="WorkflowExpressionInput"/>.
/// </summary>
public class WorkflowExpressionEditorViewModel
{
    /// <summary>
    /// The name of the bound property, from <c>Html.NameFor()</c>.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The id of the bound property, from <c>Html.IdFor()</c>.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The label of the input.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// The hint shown below the input.
    /// </summary>
    public string Hint { get; set; }

    /// <summary>
    /// The current expression.
    /// </summary>
    public WorkflowExpressionInput Value { get; set; } = new();

    /// <summary>
    /// The syntax selected when <see cref="Value"/> has none.
    /// </summary>
    public string DefaultSyntax { get; set; } = WorkflowExpressionSyntaxes.JavaScript;

    /// <summary>
    /// The syntaxes the input accepts, or none for every registered syntax.
    /// </summary>
    public IList<string> Syntaxes { get; set; } = [];

    /// <summary>
    /// An example for each syntax, by syntax name, shown as the placeholder of a one-line input.
    /// </summary>
    public IDictionary<string, string> Examples { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Whether the expression is edited in a multi-line code editor whose language follows the syntax, rather
    /// than in a one-line input.
    /// </summary>
    public bool Multiline { get; set; }

    /// <summary>
    /// Whether the label shows the input as required.
    /// </summary>
    public bool Required { get; set; }
}
