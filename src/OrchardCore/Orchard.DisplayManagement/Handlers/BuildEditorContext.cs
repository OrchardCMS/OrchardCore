using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.DisplayManagement.Handlers
{
    public class BuildEditorContext : BuildShapeContext
    {
        public BuildEditorContext(IShape shape, string groupId, string htmlFieldPrefix, IShapeFactory shapeFactory, IShape layout, IUpdateModel updater)
            : base(shape, groupId, shapeFactory, layout, updater)
        {
            HtmlFieldPrefix = htmlFieldPrefix;
        }
    }
}
