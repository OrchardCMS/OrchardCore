using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

/// <summary>
/// The posted value of an expression editor (the <c>WorkflowExpressionEditor</c> shape): its text and syntax.
/// </summary>
public sealed class WorkflowExpressionInput
{
    /// <summary>
    /// The text of the expression.
    /// </summary>
    public string Expression { get; set; }

    /// <summary>
    /// The name of its syntax.
    /// </summary>
    public string Syntax { get; set; }

    /// <summary>
    /// The input of an expression.
    /// </summary>
    public static WorkflowExpressionInput From<T>(WorkflowExpression<T> expression)
        => new()
        {
            Expression = expression?.Expression,
            Syntax = expression?.Syntax,
        };
}
