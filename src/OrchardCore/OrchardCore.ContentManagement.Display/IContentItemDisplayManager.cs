using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.ContentManagement.Display
{
    /// <summary>
    /// Describe services responsible for displaying a content item. The result dynamic objects
    /// are the Shape to render a <see cref="ContentItem"/>.
    /// </summary>
    public interface IContentItemDisplayManager
    {
        Task<dynamic> BuildDisplayAsync(ContentItem content, IUpdateModel updater, string displayType = "", string groupId = "");
        Task<dynamic> BuildEditorAsync(ContentItem content, IUpdateModel updater, string groupId = "", string htmlFieldPrefix = "");
        Task<dynamic> UpdateEditorAsync(ContentItem content, IUpdateModel updater, string groupId = "", string htmlFieldPrefix = "");
    }
}