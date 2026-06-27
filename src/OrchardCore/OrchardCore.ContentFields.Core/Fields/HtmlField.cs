using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields;

public class HtmlField : ContentField, IHtmlHolderContent
{
    public string Html { get; set; }
}
