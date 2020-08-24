using System.Threading.Tasks;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.ContentManagement.Display
{
    /// <summary>
    /// Describe services responsible for displaying a content part. The result dynamic objects
    /// are the Shape to render a <see cref="ContentPart"/>.
    /// </summary>
    public interface IContentPartComponentManager
    {
        Task<IShape> BuildDisplayAsync(ContentPart contentPart, IUpdateModel updater, string displayType = "", string groupId = "");
    }
}
