using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("shape", Attributes = nameof(Type))]
    [HtmlTargetElement("shape", Attributes = PropertyPrefix + "*")]
    public class ShapeTagHelper : BaseShapeTagHelper
    {
        public ShapeTagHelper(IShapeScopeManager shapeScopeManager, IShapeFactory shapeFactory, IDisplayHelper displayHelper)
            : base(shapeScopeManager, shapeFactory, displayHelper)
        {
        }
    }
}
