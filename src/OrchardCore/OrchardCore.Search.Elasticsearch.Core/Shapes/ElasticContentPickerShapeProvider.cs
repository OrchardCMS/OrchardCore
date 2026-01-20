using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Search.Elasticsearch;

public class ElasticContentPickerShapeProvider : IShapeAttributeProvider
{
    protected readonly IStringLocalizer S;

    public ElasticContentPickerShapeProvider(IStringLocalizer<ElasticContentPickerShapeProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    [Shape]
#pragma warning disable CA1707 // Remove the underscores from member name
    public IHtmlContent ContentPickerField_Option__Elasticsearch(dynamic shape)
#pragma warning restore CA1707
    {
        var selected = shape.Editor == "Elasticsearch";
        if (selected)
        {
            return new HtmlString($"<option value=\"Elasticsearch\" selected=\"selected\">{S["Elasticsearch"]}</option>");
        }

        return new HtmlString($"<option value=\"Elasticsearch\">{S["Elasticsearch"]}</option>");
    }
}
