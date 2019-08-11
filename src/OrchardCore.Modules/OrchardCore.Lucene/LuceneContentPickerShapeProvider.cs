using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Lucene
{
    public class LuceneContentPickerShapeProvider : IShapeAttributeProvider
    {
        public LuceneContentPickerShapeProvider(IStringLocalizer<LuceneContentPickerShapeProvider> stringLocalizer)
        {
            T = stringLocalizer;
        }

        private IStringLocalizer T { get; }

        [Shape]
        public IHtmlContent ContentPickerField_Option__Lucene(dynamic shape)
        {
            var selected = shape.Editor == "Lucene";
            if (selected)
            {
                return new HtmlString($"<option value=\"Lucene\" selected=\"selected\">{T["Lucene"]}</option>");
            }
            return new HtmlString($"<option value=\"Lucene\">{T["Lucene"]}</option>");
        }
    }
}
