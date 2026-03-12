using System.ComponentModel;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.Html.Settings;

public class HtmlFieldSettings : FieldSettings
{
    [DefaultValue(true)]
    public bool SanitizeHtml { get; set; } = true;
}
