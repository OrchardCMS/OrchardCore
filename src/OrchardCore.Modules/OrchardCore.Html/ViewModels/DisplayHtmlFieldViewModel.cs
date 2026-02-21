using OrchardCore.Html.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Html.ViewModels;

public class DisplayHtmlFieldViewModel
{
    public string Html { get; set; }

    public HtmlField Field { get; set; }

    public ContentPart Part { get; set; }

    public ContentPartFieldDefinition PartFieldDefinition { get; set; }
}
