using Microsoft.AspNetCore.Razor.TagHelpers;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.TagHelpers;

namespace Orchard.Menu.TagHelper
{
	[HtmlTargetElement("baz")]
	public class BazTagHelper : BaseShapeTagHelper
    {
		public BazTagHelper(IShapeFactory shapeFactory, IDisplayHelperFactory displayHelperFactory):
			base(shapeFactory, displayHelperFactory)
		{
			Type = "Baz";
		}
    }
}
