using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.TagHelpers;

namespace OrchardCore.Demo.TagHelpers
{
    [HtmlTargetElement("baz")]
    public class BazTagHelper : BaseShapeTagHelper
    {
        public BazTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper) :
            base(shapeFactory, displayHelper)
        {
            Type = "Baz";
        }
    }
}
