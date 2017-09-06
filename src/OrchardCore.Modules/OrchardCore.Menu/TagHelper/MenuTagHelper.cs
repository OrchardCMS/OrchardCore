using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.TagHelpers;

namespace OrchardCore.Menu.TagHelpers
{
	[HtmlTargetElement("menu")]
	public class MenuTagHelper : BaseShapeTagHelper
	{
		public MenuTagHelper(IShapeFactory shapeFactory, IDisplayHelperFactory displayHelperFactory):
			base(shapeFactory, displayHelperFactory)
		{
			Type = "Menu";
		}
    }
}
