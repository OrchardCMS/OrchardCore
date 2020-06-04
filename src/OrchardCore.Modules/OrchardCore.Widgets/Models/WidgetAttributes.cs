using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.Widgets.Models
{
    public class WidgetAttributes : ContentPart
    {
        public Dictionary<string, string> Attributes { get; set; }
    }
}