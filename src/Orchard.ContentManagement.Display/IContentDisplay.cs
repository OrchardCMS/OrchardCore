using Orchard.DependencyInjection;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display
{
    public interface IContentDisplay : ITransientDependency
    {
        Task<dynamic> BuildDisplayAsync(IContent content, string displayType = "", string groupId = "");
        Task<dynamic> BuildEditorAsync(IContent content, string groupId = "");
        Task<dynamic> UpdateEditorAsync(IContent content, string groupId = "");
    }
}