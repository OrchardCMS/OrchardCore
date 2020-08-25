using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("slot", Attributes = "name")]
    public class SlotTagHelper : TagHelper
    {
        private readonly IShapeScopeManager _shapeScopeManager;
        private readonly IDisplayHelper _displayHelper;

        [HtmlAttributeName("name")]
        public string Name { get; set; }

        public SlotTagHelper(IShapeScopeManager shapeScopeManager, IDisplayHelper displayHelper)
        {
            _shapeScopeManager = shapeScopeManager;
            _displayHelper = displayHelper;

        }
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Retrieve from slot and render. It maybe a shape, or it maybe IHtmlContent.

            output.Content.SetHtmlContent(_shapeScopeManager.GetSlot(Name));

            // We don't want any encapsulating tag around the shape
            output.TagName = null;

            return Task.CompletedTask;
        }
    }
}
