using Microsoft.AspNetCore.Razor.TagHelpers;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.TagHelpers;

namespace Orchard.Menu.TagHelper
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
