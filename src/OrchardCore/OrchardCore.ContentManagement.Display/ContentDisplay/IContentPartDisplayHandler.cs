using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    /// <summary>
    /// An implementation of <see cref="IContentDisplayHandler"/> is able to take part in the rendering of
    /// a <see cref="ContentItem"/> instance.
    /// </summary>
    public interface IContentPartDisplayHandler
    {
        Task BuildDisplayAsync(ContentPart contentItem, BuildDisplayContext context);
    }
}
