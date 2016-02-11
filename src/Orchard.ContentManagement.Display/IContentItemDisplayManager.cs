using Orchard.DependencyInjection;
using Orchard.DisplayManagement.ModelBinding;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display
{
    /// <summary>
    /// Describe services responsible for displaying a content item. The result dynamic objects
    /// are the Shape to render a <see cref="ContentItem"/>.
    /// </summary>
    public interface IContentItemDisplayManager : ITransientDependency
    {
        Task<dynamic> BuildDisplayAsync(ContentItem content, IUpdateModel updater, string displayType = "", string groupId = "");
        Task<dynamic> BuildEditorAsync(ContentItem content, IUpdateModel updater, string groupId = "");
        Task<dynamic> UpdateEditorAsync(ContentItem content, IUpdateModel updater, string groupId = "");
    }
}