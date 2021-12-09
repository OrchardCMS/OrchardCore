using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DisplayManagement.Handlers
{
    public interface IDisplayDriver<in TModel, TDisplayContext, TEditorContext, TUpdateContext>
        where TDisplayContext : BuildDisplayContext
        where TEditorContext : BuildEditorContext
        where TUpdateContext : UpdateEditorContext
    {
        Task<IDisplayResult> BuildDisplayAsync(TModel model, TDisplayContext context);
        Task<IDisplayResult> BuildEditorAsync(TModel model, TEditorContext context);
        Task<IDisplayResult> UpdateEditorAsync(TModel model, TUpdateContext context);
    }

    public interface IDisplayDriver<in TModel> : IDisplayDriver<TModel, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>
    {
    }
}
