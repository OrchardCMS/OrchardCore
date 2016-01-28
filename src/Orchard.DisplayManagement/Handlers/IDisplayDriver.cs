using Orchard.DisplayManagement.Views;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Handlers
{
    public interface IDisplayDriver<TModel>
    {
        Task<IDisplayResult> BuildDisplayAsync(TModel model, BuildDisplayContext context);
        Task<IDisplayResult> BuildEditorAsync(TModel model, BuildEditorContext context);
        Task<IDisplayResult> UpdateEditorAsync(TModel model, UpdateEditorContext context);
    }
}
