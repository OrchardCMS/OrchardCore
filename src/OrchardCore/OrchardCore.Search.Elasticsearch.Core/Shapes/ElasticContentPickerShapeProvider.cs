using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Modules;

namespace OrchardCore.Search.Elasticsearch
{
    [Feature("OrchardCore.Search.Elasticsearch.ContentPicker")]
    public class ElasticContentPickerShapeProvider : IShapeAttributeProvider
    {
        protected readonly IStringLocalizer S;

        public ElasticContentPickerShapeProvider(IStringLocalizer<ElasticContentPickerShapeProvider> stringLocalizer)
        {
            S = stringLocalizer;
        }

        [Shape]
        public IHtmlContent ContentPickerField_Option__Elasticsearch(dynamic shape)
        {
            var selected = shape.Editor == "Elasticsearch";
            if (selected)
            {
                return new HtmlString($"<option value=\"Elasticsearch\" selected=\"selected\">{S["Elasticsearch"]}</option>");
            }

            return new HtmlString($"<option value=\"Elasticsearch\">{S["Elasticsearch"]}</option>");
        }
    }
}
