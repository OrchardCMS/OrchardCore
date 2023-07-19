using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Modules;

namespace OrchardCore.Search.Lucene
{
    [Feature("OrchardCore.Search.Lucene.ContentPicker")]
    public class LuceneContentPickerShapeProvider : IShapeAttributeProvider
    {
        protected readonly IStringLocalizer S;

        public LuceneContentPickerShapeProvider(IStringLocalizer<LuceneContentPickerShapeProvider> stringLocalizer)
        {
            S = stringLocalizer;
        }

        [Shape]
        public IHtmlContent ContentPickerField_Option__Lucene(dynamic shape)
        {
            var selected = shape.Editor == "Lucene";
            if (selected)
            {
                return new HtmlString($"<option value=\"Lucene\" selected=\"selected\">{S["Lucene"]}</option>");
            }
            return new HtmlString($"<option value=\"Lucene\">{S["Lucene"]}</option>");
        }
    }
}
