namespace OrchardCore.Workflows.Models;

/// <summary>
/// The names of the built-in expression syntaxes (see <see cref="WorkflowExpression{T}.Syntax"/>).
/// </summary>
public static class WorkflowExpressionSyntaxes
{
    /// <summary>
    /// The text is the value itself, converted to the expected type.
    /// </summary>
    public const string Literal = "Literal";

    /// <summary>
    /// A Liquid template.
    /// </summary>
    public const string Liquid = "Liquid";

    /// <summary>
    /// A JavaScript expression.
    /// </summary>
    public const string JavaScript = "JavaScript";

    /// <summary>
    /// The syntax name of a <see cref="WorkflowScriptSyntax"/>, the syntax setting of the activities that
    /// predate per-expression syntaxes.
    /// </summary>
    public static string From(WorkflowScriptSyntax syntax)
        => syntax == WorkflowScriptSyntax.Liquid ? Liquid : JavaScript;

    /// <summary>
    /// The expression an activity evaluates: <paramref name="expression"/> when it has a syntax, otherwise the one
    /// of the legacy pair that <paramref name="legacySyntax"/> selects, with that syntax.
    /// </summary>
    /// <param name="expression">The expression, which holds the JavaScript text of a legacy pair.</param>
    /// <param name="legacyLiquid">The Liquid text of a legacy pair, or <see langword="null"/>.</param>
    /// <param name="legacySyntax">The legacy syntax setting of the activity.</param>
    public static WorkflowExpression<T> Resolve<T>(WorkflowExpression<T> expression, string legacyLiquid, WorkflowScriptSyntax legacySyntax)
    {
        if (!string.IsNullOrEmpty(expression?.Syntax))
        {
            return expression;
        }

        return legacySyntax == WorkflowScriptSyntax.Liquid
            ? new WorkflowExpression<T>(legacyLiquid, Liquid)
            : new WorkflowExpression<T>(expression?.Expression, JavaScript);
    }
}
