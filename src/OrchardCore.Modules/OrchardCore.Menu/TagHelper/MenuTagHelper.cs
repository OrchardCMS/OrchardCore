using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.TagHelpers;

namespace OrchardCore.Menu.TagHelpers
{
    [HtmlTargetElement("menu")]
    public class MenuTagHelper : BaseShapeTagHelper
    {
        public MenuTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper) :
            base(shapeFactory, displayHelper)
        {
            Type = "Menu";
        }
    }
}
