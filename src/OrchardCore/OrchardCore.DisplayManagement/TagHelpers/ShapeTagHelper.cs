using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("shape", Attributes = nameof(Type))]
    [HtmlTargetElement("shape", Attributes = PropertyPrefix + "*")]
    public class ShapeTagHelper : BaseShapeTagHelper
    {
        public ShapeTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper)
            : base(shapeFactory, displayHelper)
        {
        }
    }
}
