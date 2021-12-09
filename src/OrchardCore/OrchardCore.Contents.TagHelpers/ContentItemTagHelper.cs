using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.TagHelpers;

namespace OrchardCore.Contents.TagHelpers
{
    [HtmlTargetElement("contentitem")]
    public class ContentItemTagHelper : BaseShapeTagHelper
    {
        public ContentItemTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper)
            : base(shapeFactory, displayHelper)
        {
            Type = "ContentItem";
        }
    }
}
