using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    /// <summary>
    /// An implementation of <see cref="IContentDisplayHandler"/> is able to take part in the rendering of
    /// a <see cref="ContentItem"/> instance.
    /// </summary>
    public interface IContentDisplayHandler
    {
        Task BuildDisplayAsync(ContentItem contentItem, BuildDisplayContext context, ContentTypeDefinition contentTypeDefinition);
        Task BuildEditorAsync(ContentItem contentItem, BuildEditorContext context, ContentTypeDefinition contentTypeDefinition);
        Task UpdateEditorAsync(ContentItem contentItem, UpdateEditorContext context, ContentTypeDefinition contentTypeDefinition);
    }
}
