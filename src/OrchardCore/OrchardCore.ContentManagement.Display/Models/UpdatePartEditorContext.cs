using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentManagement.Display.Models
{
    public class UpdatePartEditorContext : BuildPartEditorContext
    {

        public UpdatePartEditorContext(ContentTypePartDefinition typePartDefinition, UpdateEditorContext context)
            : base(typePartDefinition, context)
        {
        }

    }
}
