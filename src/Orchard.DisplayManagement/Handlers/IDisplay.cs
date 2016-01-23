using Orchard.DependencyInjection;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Handlers
{
    public interface IDisplay<TModel>
    {
        Task<DisplayResult<TModel>> BuildDisplayAsync(BuildDisplayContext<TModel> context);
        Task<DisplayResult<TModel>> BuildEditorAsync(BuildEditorContext<TModel> context);
        Task<DisplayResult<TModel>> UpdateEditorAsync(UpdateEditorContext<TModel> context, IUpdateModel updater);
    }
    
}
