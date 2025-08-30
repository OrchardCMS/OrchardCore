using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentManagement.Display.Models;

public class BuildPartEditorContext : BuildEditorContext
{
    public BuildPartEditorContext(
        ContentTypePartDefinition typePartDefinition,
        BuildEditorContext context) : base(context.Shape, context.GroupId, context.IsNew, "", context.ShapeFactory, context.Layout, context.Updater, context.HttpContext)
    {
        TypePartDefinition = typePartDefinition;
    }

    public ContentTypePartDefinition TypePartDefinition { get; }
    public string PartLocation { get; set; }
}
