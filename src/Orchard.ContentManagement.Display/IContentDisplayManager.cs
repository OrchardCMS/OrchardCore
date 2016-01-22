using Orchard.DependencyInjection;
using Orchard.DisplayManagement.ModelBinding;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display
{
    public interface IContentDisplayManager : ITransientDependency
    {
        Task<dynamic> BuildDisplayAsync(IContent content, string displayType = "", string groupId = "");
        Task<dynamic> BuildEditorAsync(IContent content, string groupId = "");
        Task<dynamic> UpdateEditorAsync(IContent content, IUpdateModel updater, string groupId = "");
    }
}