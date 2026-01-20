using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Handlers
{
    public interface IDisplayHandler<TModel>
    {
        Task BuildDisplayAsync(TModel model, BuildDisplayContext context);
        Task BuildEditorAsync(TModel model, BuildEditorContext context);
        Task UpdateEditorAsync(TModel model, UpdateEditorContext context);
    }
}
