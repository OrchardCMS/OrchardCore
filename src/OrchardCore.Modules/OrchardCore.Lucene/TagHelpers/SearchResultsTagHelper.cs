using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.TagHelpers;

namespace OrchardCore.Lucene.TagHelpers
{
    [HtmlTargetElement("searchresults")]
    public class SearchResultsTagHelper : BaseShapeTagHelper
    {
        public SearchResultsTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper) :
            base(shapeFactory, displayHelper)
        {
            Type = "Search__Results";
        }
    }
}
