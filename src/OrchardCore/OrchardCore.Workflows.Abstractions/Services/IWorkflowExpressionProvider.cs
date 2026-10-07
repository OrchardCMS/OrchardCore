using System.Text.Encodings.Web;
using Microsoft.Extensions.Localization;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// An expression syntax, such as Literal, Liquid or JavaScript. Register implementations as
/// <see cref="IWorkflowExpressionProvider"/> services; a later registration of a name replaces an earlier one.
/// </summary>
public interface IWorkflowExpressionProvider
{
    /// <summary>
    /// The name of the syntax, stored in <see cref="WorkflowExpression{T}.Syntax"/>.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The name shown in the expression editors.
    /// </summary>
    LocalizedString DisplayName { get; }

    /// <summary>
    /// The Monaco language of the multi-line editor, such as <c>plaintext</c>, <c>liquid</c> or <c>javascript</c>.
    /// </summary>
    string EditorLanguage { get; }

    /// <summary>
    /// Evaluates <paramref name="expression"/> and converts its value to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <param name="workflowContext">The workflow that evaluates it.</param>
    /// <param name="context">Options of the evaluation, or <see langword="null"/>.</param>
    Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, WorkflowExpressionEvaluationContext context);

    /// <summary>
    /// Returns what is wrong with the text of an expression, or an empty list.
    /// </summary>
    /// <param name="expression">The text of the expression.</param>
    /// <param name="valueType">The type the activity expects the value to have.</param>
    IReadOnlyList<string> Validate(string expression, Type valueType);
}

/// <summary>
/// Options of an expression evaluation.
/// </summary>
public sealed class WorkflowExpressionEvaluationContext
{
    /// <summary>
    /// The encoder of the values a Liquid template outputs, or <see langword="null"/> for none.
    /// </summary>
    public TextEncoder Encoder { get; init; }

    /// <summary>
    /// Additional JavaScript functions, such as <c>setOutcome()</c>.
    /// </summary>
    public IGlobalMethodProvider[] MethodProviders { get; init; } = [];
}
