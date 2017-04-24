using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentManagement.Display.Models
{
    public class BuildPartEditorContext : BuildEditorContext
    {
        public BuildPartEditorContext(ContentTypePartDefinition typePartDefinition, BuildEditorContext context)
            : base(context.Shape, context.GroupId, "", context.ShapeFactory, context.Layout, context.Updater)
        {
            TypePartDefinition = typePartDefinition;
        }

        public ContentTypePartDefinition TypePartDefinition { get; }
        public string PartLocation { get; set; }
    }
}
