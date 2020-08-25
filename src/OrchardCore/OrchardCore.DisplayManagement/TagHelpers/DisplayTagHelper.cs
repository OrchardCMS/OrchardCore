using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("display", Attributes = "Shape")]
    public class DisplayTagHelper : BaseShapeTagHelper
    {
        public DisplayTagHelper(IShapeScopeManager shapeScopeManager, IShapeFactory shapeFactory, IDisplayHelper displayHelper)
            : base(shapeScopeManager, shapeFactory, displayHelper)
        {
        }
    }
}
