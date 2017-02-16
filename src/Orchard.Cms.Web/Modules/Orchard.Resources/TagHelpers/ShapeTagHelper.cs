using Microsoft.AspNetCore.Razor.TagHelpers;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.TagHelpers;

namespace Orchard.Resources.TagHelpers
{
	[HtmlTargetElement("shape", Attributes = nameof(Type))]
    public class ShapeTagHelper : BaseShapeTagHelper
    {
        public ShapeTagHelper(IShapeFactory shapeFactory, IDisplayHelperFactory displayHelperFactory)
			:base(shapeFactory, displayHelperFactory)
        {
            _shapeFactory = shapeFactory;
            _displayHelperFactory = displayHelperFactory;
        }
    }
}