using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Html.Fields;

namespace OrchardCore.Html.ViewModels;

public class EditHtmlFieldViewModel
{
    public string Html { get; set; }

    public HtmlField Field { get; set; }

    public ContentPart Part { get; set; }

    public ContentPartFieldDefinition PartFieldDefinition { get; set; }
}
