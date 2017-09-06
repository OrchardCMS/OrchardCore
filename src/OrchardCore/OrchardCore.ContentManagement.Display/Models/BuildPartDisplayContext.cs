using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentManagement.Display.Models
{
    public class BuildPartDisplayContext : BuildDisplayContext
    {
        public BuildPartDisplayContext(ContentTypePartDefinition typePartDefinition, BuildDisplayContext context)
            : base(context.Shape, context.DisplayType, context.GroupId, context.ShapeFactory, context.Layout, context.Updater)
        {
            TypePartDefinition = typePartDefinition;
        }

        public ContentTypePartDefinition TypePartDefinition { get; }
    }
}
