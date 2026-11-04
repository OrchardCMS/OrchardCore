using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Markdown.Fields;

public class MarkdownField : ContentField
{
    [Description("The Markdown content.")]
    public string Markdown { get; set; }
}
