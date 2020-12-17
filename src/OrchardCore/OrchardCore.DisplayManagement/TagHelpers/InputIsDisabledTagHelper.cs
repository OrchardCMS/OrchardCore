using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("input", Attributes = ForAndIsDisabledAttributeName)]
    public class InputIsDisabledTagHelper : TagHelper
    {
        private const string ForAndIsDisabledAttributeName = "asp-for, asp-is-disabled";

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
