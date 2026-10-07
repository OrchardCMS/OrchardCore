using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentManagement.Display.ViewModels;

public class ContentPartShapeViewModel : ShapeViewModel
{
    public ContentPartShapeViewModel()
    {
    }

    public ContentPart ContentPart { get; set; }

    public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
}
