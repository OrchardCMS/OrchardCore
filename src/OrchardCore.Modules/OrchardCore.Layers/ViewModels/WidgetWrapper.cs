using OrchardCore.ContentManagement;
using OrchardCore.Layers.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Layers.ViewModels;

public class WidgetWrapper : ShapeViewModel
{
    public WidgetWrapper() : base("Widget_Wrapper")
    {
    }

    public ContentItem Widget { get; set; }
    public IShape Content { get; set; }

    /// <summary>
    /// Gets or sets the layer metadata of the widget, when already known by the code that renders it.
    /// </summary>
    public LayerMetadata LayerMetadata { get; set; }
}
