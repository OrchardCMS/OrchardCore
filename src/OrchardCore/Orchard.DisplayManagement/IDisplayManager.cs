using Orchard.DisplayManagement.ModelBinding;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement
{
    public interface IDisplayManager<TModel>
    {
        Task<dynamic> BuildDisplayAsync(TModel model, IUpdateModel updater, string displayType = "", string groupId = "");
        Task<dynamic> BuildEditorAsync(TModel model, IUpdateModel updater, string groupId = "");
        Task<dynamic> UpdateEditorAsync(TModel model, IUpdateModel updater, string groupId = "");
    }
}