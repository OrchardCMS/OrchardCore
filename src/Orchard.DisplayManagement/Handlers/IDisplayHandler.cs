using Orchard.DependencyInjection;
using Orchard.DisplayManagement.ModelBinding;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Handlers
{
    public interface IDisplayHandler<TModel>
    {
        Task BuildDisplayAsync(BuildDisplayContext<TModel> context);
        Task BuildEditorAsync(BuildEditorContext<TModel> context);
        Task UpdateEditorAsync(UpdateEditorContext<TModel> context, IUpdateModel modelUpdater);
    }
}
