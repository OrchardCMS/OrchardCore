using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.TagHelpers;

namespace OrchardCore.Lucene.TagHelpers
{
    [HtmlTargetElement("searchform")]
    public class SearchFormTagHelper : BaseShapeTagHelper
    {
        public SearchFormTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper) :
            base(shapeFactory, displayHelper)
        {
            Type = "Search__Form";
        }
    }
}
