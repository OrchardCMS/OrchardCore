using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Search.Lucene;

public class LuceneContentPickerShapeProvider : IShapeAttributeProvider
{
    protected readonly IStringLocalizer S;

    public LuceneContentPickerShapeProvider(IStringLocalizer<LuceneContentPickerShapeProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    [Shape]
#pragma warning disable CA1707 // Remove the underscores from member name
    public IHtmlContent ContentPickerField_Option__Lucene(dynamic shape)
#pragma warning restore CA1707
    {
        var selected = shape.Editor == "Lucene";
        if (selected)
        {
            return new HtmlString($"<option value=\"Lucene\" selected=\"selected\">{S["Lucene"]}</option>");
        }
        return new HtmlString($"<option value=\"Lucene\">{S["Lucene"]}</option>");
    }
}
