using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Models;

namespace OrchardCore.ContentManagement.Display.Models
{
    public class BuildFieldDisplayContext : BuildDisplayContext, IFieldDisplayManagementContext
    {
        public BuildFieldDisplayContext(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, ContentPartFieldDefinition partFieldDefinition, BuildDisplayContext context, bool PartTemplatedField = false)
            : base(context.Shape, context.DisplayType, context.GroupId, context.ShapeFactory, context.Layout, context.Updater)
        {
            ContentPart = contentPart;
            TypePartDefinition = typePartDefinition;
            PartFieldDefinition = partFieldDefinition;

            if(PartTemplatedField)// rendered from within Part template instead of entire zone
                (this as IFieldDisplayManagementContext).ExplicitPartName = typePartDefinition.Name;
        }

        public ContentPart ContentPart { get; }
        public ContentTypePartDefinition TypePartDefinition { get; }
        public ContentPartFieldDefinition PartFieldDefinition { get; }

        string IFieldDisplayManagementContext.ExplicitPartName { get; set; }
    }
}
