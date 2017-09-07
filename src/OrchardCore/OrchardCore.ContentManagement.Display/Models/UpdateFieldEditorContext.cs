using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentManagement.Display.Models
{
    public class UpdateFieldEditorContext : BuildFieldEditorContext
    {
        public UpdateFieldEditorContext(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, ContentPartFieldDefinition partFieldDefinition, UpdateEditorContext context)
            : base(contentPart, typePartDefinition, partFieldDefinition, context)
        {
        }
    }
}
