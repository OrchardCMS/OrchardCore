using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// The registered expression syntaxes, and the evaluation of an expression with the one it uses.
/// </summary>
public interface IWorkflowExpressionManager
{
    /// <summary>
    /// Returns every syntax: Literal, Liquid and JavaScript first, then the others by name.
    /// </summary>
    IReadOnlyList<IWorkflowExpressionProvider> List();

    /// <summary>
    /// Returns the syntax with the given name (ignoring case), or <see langword="null"/>.
    /// </summary>
    /// <param name="syntax">The <see cref="IWorkflowExpressionProvider.Name"/>.</param>
    IWorkflowExpressionProvider Get(string syntax);

    /// <summary>
    /// Evaluates <paramref name="expression"/> with the provider of its <see cref="WorkflowExpression{T}.Syntax"/>,
    /// or of <paramref name="defaultSyntax"/> when it has none.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <param name="workflowContext">The workflow that evaluates it.</param>
    /// <param name="defaultSyntax">The syntax of an expression that has none.</param>
    /// <param name="context">Options of the evaluation, or <see langword="null"/>.</param>
    /// <exception cref="NotSupportedException">No provider has the syntax of the expression.</exception>
    Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, string defaultSyntax = null, WorkflowExpressionEvaluationContext context = null);
}
