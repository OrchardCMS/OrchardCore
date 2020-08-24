using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.TagHelpers;

namespace OrchardCore.Demo.TagHelpers
{
    [HtmlTargetElement("baz")]
    public class BazTagHelper : BaseShapeTagHelper
    {
        public BazTagHelper(IShapeScopeManager shapeScopeManager, IShapeFactory shapeFactory, IDisplayHelper displayHelper) :
            base(shapeScopeManager, shapeFactory, displayHelper)
        {
            Type = "Baz";
        }
    }
}
