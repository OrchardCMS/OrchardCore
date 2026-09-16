using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Widgets.Models;

public class WidgetMetadata : ContentPart
{
    [Description("Whether the widget's title is rendered.")]
    public bool RenderTitle { get; set; }

    [Description("The widget's ordering position within its zone.")]
    public string Position { get; set; }
}
