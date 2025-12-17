using OrchardCore.DisplayManagement;

namespace OrchardCore.Widgets.Models;

public class ContentCardFrameShape
{
    public IShape ChildContent { get; set; }
    public int? ColumnSize { get; set; }
}
