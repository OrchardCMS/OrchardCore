namespace OrchardCore.Workflows.Models;

/// <summary>
/// An expression an activity evaluates while it runs, such as a condition or a value.
/// </summary>
/// <typeparam name="T">The type of the expression's value.</typeparam>
public class WorkflowExpression<T>
{
    public WorkflowExpression()
    {
    }

    public WorkflowExpression(string expression)
    {
        Expression = expression;
    }

    public WorkflowExpression(string expression, string syntax)
    {
        Expression = expression;
        Syntax = syntax;
    }

    /// <summary>
    /// The text of the expression.
    /// </summary>
    public string Expression { get; set; }

    /// <summary>
    /// The syntax of <see cref="Expression"/>, the name of an <see cref="Services.IWorkflowExpressionProvider"/>
    /// (see <see cref="WorkflowExpressionSyntaxes"/>), or <see langword="null"/> when the activity decides, as for
    /// the expressions stored before a syntax could be chosen for each one.
    /// </summary>
    public string Syntax { get; set; }

    public override string ToString()
    {
        return Expression;
    }
}
