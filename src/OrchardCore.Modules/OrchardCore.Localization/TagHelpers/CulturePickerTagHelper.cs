using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.TagHelpers;

namespace OrchardCore.Localization.TagHelpers
{
	[HtmlTargetElement("culture-picker")]
	public class CulturePickerTagHelper : BaseShapeTagHelper
	{
		public CulturePickerTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper):
			base(shapeFactory, displayHelper)
		{

		}
    }
}