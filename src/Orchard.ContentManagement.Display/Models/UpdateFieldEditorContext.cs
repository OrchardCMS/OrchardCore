using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentManagement.Display.Models
{
    public class UpdateFieldEditorContext : BuildFieldEditorContext
    {
        public UpdateFieldEditorContext(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, ContentPartFieldDefinition partFieldDefinition, UpdateEditorContext context)
            : base(contentPart, typePartDefinition, partFieldDefinition, context)
        {
        }
    }
}
