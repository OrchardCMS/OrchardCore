using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Handlers
{
    public interface IDisplayHandler<TModel>
    {
        Task BuildDisplayAsync(TModel model, BuildDisplayContext context);
        Task BuildEditorAsync(TModel model, BuildEditorContext context);
        Task UpdateEditorAsync(TModel model, UpdateEditorContext context);
    }
}
