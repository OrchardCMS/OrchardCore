using OrchardCore.DisplayManagement;

namespace OrchardCore.ContentTypes.Shapes;

public class ContentCardFrameShape
{
    public IShape ChildContent { get; set; }
    public int? ColumnSize { get; set; }
}
