using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public interface IContentPartDisplayDriver
    {
        Task<IDisplayResult> BuildDisplayAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, BuildDisplayContext context);
        Task<IDisplayResult> BuildEditorAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, BuildEditorContext context);
        Task<IDisplayResult> UpdateEditorAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, UpdateEditorContext context);
    }
}
