using OrchardCore.DisplayManagement.ModelBinding;
using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement
{
    public interface IDisplayManager<TModel>
    {
        Task<IShape> BuildDisplayAsync(TModel model, IUpdateModel updater, string displayType = "", string groupId = "");
        Task<IShape> BuildEditorAsync(TModel model, IUpdateModel updater, bool isNew, string groupId = "");
        Task<IShape> UpdateEditorAsync(TModel model, IUpdateModel updater, bool isNew, string groupId = "");
    }
}