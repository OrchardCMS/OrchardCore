using System.ComponentModel;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.Markdown.Settings;

public class MarkdownFieldSettings : FieldSettings
{
    [DefaultValue(true)]
    public bool SanitizeHtml { get; set; } = true;
}
