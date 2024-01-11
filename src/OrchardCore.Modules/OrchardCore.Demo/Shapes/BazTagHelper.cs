using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.Demo.TagHelpers.Intellisense
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
