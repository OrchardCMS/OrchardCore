using Microsoft.AspNetCore.Razor.TagHelpers;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.TagHelpers;

namespace Orchard.Demo.TagHelpers
{
    [HtmlTargetElement("baz", Attributes = nameof(Text))]
    public class BazTagHelper : ShapeTagHelper
    {
        public BazTagHelper(IShapeFactory shapeFactory, IDisplayHelperFactory displayHelperFactory)
            : base(shapeFactory, displayHelperFactory)
        {
            Type = "Baz";
        }

        /// <summary>
        /// The text to render
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// the number of times to repeat the text
        /// </summary>
        public int Count { get; set; }
    }
}