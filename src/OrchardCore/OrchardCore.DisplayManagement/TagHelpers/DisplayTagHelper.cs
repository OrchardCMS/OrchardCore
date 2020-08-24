using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("display", Attributes = nameof(Type))]
    [HtmlTargetElement("display", Attributes = PropertyPrefix + "*")]
    [HtmlTargetElement("display", Attributes = nameof(Slot))]
    public class DisplayTagHelper : BaseShapeTagHelper
    {
        public DisplayTagHelper(IShapeScopeManager shapeScopeManager, IShapeFactory shapeFactory, IDisplayHelper displayHelper)
            : base(shapeScopeManager, shapeFactory, displayHelper)
        {
        }
    }
}
