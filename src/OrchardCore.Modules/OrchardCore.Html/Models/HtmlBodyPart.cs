using OrchardCore.ContentManagement;

namespace OrchardCore.Html.Models;

public class HtmlBodyPart : ContentPart, IHtmlHolderContent
{
    public string Html { get; set; }
}
