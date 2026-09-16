using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Markdown.Models;

public class MarkdownBodyPart : ContentPart
{
    [Description("The Markdown source for the content body.")]
    public string Markdown { get; set; }
}
