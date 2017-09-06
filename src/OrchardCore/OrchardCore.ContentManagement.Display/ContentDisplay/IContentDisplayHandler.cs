using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    /// <summary>
    /// An implementation of <see cref="IContentDisplayHandler"/> is able to take part in the rendering of
    /// a <see cref="ContentItem"/> instance.
    /// </summary>
    public interface IContentDisplayHandler
    {
        Task BuildDisplayAsync(ContentItem contentItem, BuildDisplayContext context);
        Task BuildEditorAsync(ContentItem contentItem, BuildEditorContext context);
        Task UpdateEditorAsync(ContentItem contentItem, UpdateEditorContext context);
    }
}
