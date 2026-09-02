using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Html.ViewModels;

namespace OrchardCore.ContentFields.ViewModels;

public class DisplayHtmlFieldViewModel : HtmlViewModelBase
{
    public HtmlField Field { get; set; }
    public ContentPart Part { get; set; }
    public ContentPartFieldDefinition PartFieldDefinition { get; set; }
}
