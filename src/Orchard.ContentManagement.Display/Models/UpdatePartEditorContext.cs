using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentManagement.Display.Models
{
    public class UpdatePartEditorContext : BuildPartEditorContext
    {
        public UpdatePartEditorContext(ContentTypePartDefinition typePartDefinition, UpdateEditorContext context)
            : base(typePartDefinition, context)
        {
        }
    }
}
