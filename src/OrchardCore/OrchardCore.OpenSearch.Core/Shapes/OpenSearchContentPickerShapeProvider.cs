using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.OpenSearch;

public sealed class OpenSearchContentPickerShapeProvider : IShapeAttributeProvider
{
    protected readonly IStringLocalizer S;

    public OpenSearchContentPickerShapeProvider(IStringLocalizer<OpenSearchContentPickerShapeProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    [Shape]
#pragma warning disable CA1707 // Remove the underscores from member name
    public IHtmlContent ContentPickerField_Option__OpenSearch(dynamic shape)
#pragma warning restore CA1707
    {
        var selected = shape.Editor == "OpenSearch";
        if (selected)
        {
            return new HtmlString($"<option value=\"OpenSearch\" selected=\"selected\">{S["OpenSearch"]}</option>");
        }

        return new HtmlString($"<option value=\"OpenSearch\">{S["OpenSearch"]}</option>");
    }
}
