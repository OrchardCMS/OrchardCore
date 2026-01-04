using OrchardCore.DisplayManagement;

namespace OrchardCore.Flows.Models;

public class ContentCardFrameShape
{
    public IShape ChildContent { get; set; }
    public int? ColumnSize { get; set; }
}
