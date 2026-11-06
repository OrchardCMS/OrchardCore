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

    private LayerMetadata _layerMetadata;

    public ContentItem Widget { get; set; }
    public IShape Content { get; set; }

    /// <summary>
    /// Gets or sets the layer metadata of the widget. When it is not set by the code that renders the widget,
    /// it is read from the widget content item.
    /// </summary>
    public LayerMetadata LayerMetadata
    {
        get
        {
            if (_layerMetadata is null && Widget is not null)
            {
                Widget.TryGet(out _layerMetadata);
            }

            return _layerMetadata;
        }

        set => _layerMetadata = value;
    }
}
