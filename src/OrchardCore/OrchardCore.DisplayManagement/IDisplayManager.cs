using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.DisplayManagement
{
    public interface IDisplayManager<TModel>
    {
        Task<IShape> BuildDisplayAsync(TModel model, IUpdateModel updater, string displayType = "", string groupId = "");
        Task<IShape> BuildEditorAsync(TModel model, IUpdateModel updater, bool isNew, string groupId = "");
        Task<IShape> UpdateEditorAsync(TModel model, IUpdateModel updater, bool isNew, string groupId = "");
    }
}
