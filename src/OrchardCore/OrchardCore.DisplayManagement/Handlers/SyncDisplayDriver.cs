using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DisplayManagement.Handlers;

/// <summary>
/// A version of <see cref="DisplayDriver{TModel,TDisplayContext,TEditorContext,TUpdateContext}"/> where only the update
/// method is asynchronous.
/// </summary>
public abstract class SyncDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext> :
    DisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>
    where TModel : class
    where TDisplayContext : BuildDisplayContext
    where TEditorContext : BuildEditorContext
    where TUpdateContext : UpdateEditorContext, TEditorContext
{
    public new virtual IDisplayResult Display(TModel model, TDisplayContext context) => NullShapeResult();

    public new virtual IDisplayResult Edit(TModel model, TEditorContext context) => NullShapeResult();

    public sealed override Task<IDisplayResult> DisplayAsync(TModel model, TDisplayContext context) =>
        Task.FromResult(Display(model, context));

    public sealed override Task<IDisplayResult> EditAsync(TModel model, TEditorContext context) =>
        Task.FromResult(Edit(model, context));

    public override Task<IDisplayResult> UpdateAsync(TModel model, TUpdateContext context) =>
        Task.FromResult(Edit(model, context));

    private static IDisplayResult NullShapeResult() => null;
}

/// <summary>
/// A version of <see cref="DisplayDriver{TModel}"/> where only the update method is asynchronous.
/// </summary>
/// <typeparam name="TModel"></typeparam>
public abstract class SyncDisplayDriver<TModel> :
    SyncDisplayDriver<TModel, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>,
    IDisplayDriver<TModel>
    where TModel : class
{
}
