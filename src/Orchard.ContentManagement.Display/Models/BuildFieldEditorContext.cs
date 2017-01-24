using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentManagement.Display.Models
{
    public class BuildFieldEditorContext : BuildEditorContext
    {
        public BuildFieldEditorContext(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
            : base(context.Shape, context.GroupId, "", context.ShapeFactory, context.Layout, context.Updater)
        {
            ContentPart = contentPart;
            TypePartDefinition = typePartDefinition;
            PartFieldDefinition = partFieldDefinition;
        }

        public ContentPart ContentPart { get; }
        public ContentTypePartDefinition TypePartDefinition { get; }
        public ContentPartFieldDefinition PartFieldDefinition { get; }
    }
}
