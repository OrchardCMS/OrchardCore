using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentManagement.Display.Models
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
