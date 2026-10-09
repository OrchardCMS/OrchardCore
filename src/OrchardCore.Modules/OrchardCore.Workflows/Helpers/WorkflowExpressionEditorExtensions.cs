using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Helpers;

public static class WorkflowExpressionEditorExtensions
{
    /// <summary>
    /// Creates the <c>WorkflowExpressionEditor</c> shape of an expression input of the view's model. Render it
    /// with <c>DisplayAsync</c>; its fields bind back to the <see cref="WorkflowExpressionInput"/>.
    /// </summary>
    /// <param name="shapeFactory">The shape factory, <c>Factory</c> in a Razor view.</param>
    /// <param name="html">The HTML helper of the view.</param>
    /// <param name="expression">The input's property in the view's model.</param>
    /// <param name="initialize">Sets the label, hint, syntaxes and other options of the editor.</param>
    public static ValueTask<IShape> CreateWorkflowExpressionEditorAsync<TModel>(
        this IShapeFactory shapeFactory,
        IHtmlHelper<TModel> html,
        Expression<Func<TModel, WorkflowExpressionInput>> expression,
        Action<WorkflowExpressionEditorViewModel> initialize = null)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(expression);

        var name = html.NameFor(expression);
        var id = html.IdFor(expression);
        var value = html.ViewData.Model is null ? null : expression.Compile()(html.ViewData.Model);

        return shapeFactory.CreateAsync<WorkflowExpressionEditorViewModel>("WorkflowExpressionEditor", editor =>
        {
            editor.Name = name;
            editor.Id = id;
            editor.Value = value ?? new WorkflowExpressionInput();
            initialize?.Invoke(editor);
        });
    }
}
