using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("template", Attributes = "slot")]
    public class TemplateTagHelper : TagHelper
    {
        private readonly IShapeScopeManager _shapeScopeManager;

        [HtmlAttributeName("slot")]
        public string Slot { get; set; }


        public TemplateTagHelper(IShapeScopeManager shapeScopeManager)
        {
            _shapeScopeManager = shapeScopeManager;

        }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Don't render this, place it in a slot for rendering later.

            var childContent = await output.GetChildContentAsync();
            _shapeScopeManager.AddSlot(Slot, childContent);

            output.SuppressOutput();
        }
    }
}
