using Orchard.DisplayManagement.Views;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Handlers
{
    public interface IDisplayDriver<TModel, TDisplayContext, TEditorContext, TUpdateContext>
        where TDisplayContext : BuildDisplayContext
        where TEditorContext : BuildEditorContext
        where TUpdateContext : UpdateEditorContext
    {
        Task<IDisplayResult> BuildDisplayAsync(TModel model, TDisplayContext context);
        Task<IDisplayResult> BuildEditorAsync(TModel model, TEditorContext context);
        Task<IDisplayResult> UpdateEditorAsync(TModel model, TUpdateContext context);
    }
}
