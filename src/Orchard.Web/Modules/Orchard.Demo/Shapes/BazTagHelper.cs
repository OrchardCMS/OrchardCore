using Microsoft.AspNet.Razor.TagHelpers;

namespace Orchard.Demo.Shapes
{
    public class BazTagHelper : TagHelper
    {
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