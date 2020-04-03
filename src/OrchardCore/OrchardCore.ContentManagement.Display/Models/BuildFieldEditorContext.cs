using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentManagement.Display.Models
{
    public class BuildFieldEditorContext : BuildEditorContext
    {
        public BuildFieldEditorContext(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
            : base(context.Shape, context.GroupId, context.IsNew, "", context.ShapeFactory, context.Layout, context.Updater)
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
