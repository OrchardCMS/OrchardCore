using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Html.Models;

public class HtmlBodyPart : ContentPart
{
    [Description("The HTML markup for the content body.")]
    public string Html { get; set; }
}
