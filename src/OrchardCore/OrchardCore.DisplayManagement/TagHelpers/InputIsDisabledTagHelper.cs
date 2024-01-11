using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("input", Attributes = ForAttributeName + "," + IsDisabledAttributeName)]
    public class InputIsDisabledTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string IsDisabledAttributeName = "asp-is-disabled";

        [HtmlAttributeName("asp-is-disabled")]
        public bool IsDisabled { set; get; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (IsDisabled)
            {
                var d = new TagHelperAttribute("disabled", "disabled");
                output.Attributes.Add(d);
            }
        }
    }
}
