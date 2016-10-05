using System.Threading.Tasks;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public interface IContentPartDisplayDriver
    {
        Task<IDisplayResult> BuildDisplayAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, BuildDisplayContext context);
        Task<IDisplayResult> BuildEditorAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, BuildEditorContext context);
        Task<IDisplayResult> UpdateEditorAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, UpdateEditorContext context);
    }
}
