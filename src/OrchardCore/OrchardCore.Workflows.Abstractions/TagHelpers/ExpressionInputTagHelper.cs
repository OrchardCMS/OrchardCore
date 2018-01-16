using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Workflows.Abstractions.TagHelpers
{
    [HtmlTargetElement("input", Attributes = "[type=expression]", TagStructure = TagStructure.WithoutEndTag)]
    public class ExpressionInputTagHelper : InputTagHelper
    {
        public ExpressionInputTagHelper(IStringLocalizer<ExpressionInputTagHelper> localizer, IHtmlGenerator generator) : base(generator)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }
        public string Syntax { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Clear();
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "div";
            output.Attributes.Add("class", "input-group");

            var inputTextBuilder = Generator.GenerateTextBox(ViewContext, For.ModelExplorer, For.Name, For.Model, null, new { placeholder = T["Enter a {0} expression", Syntax] });
            inputTextBuilder.AddCssClass("form-control");
            inputTextBuilder.AddCssClass("workflow-expression");
            inputTextBuilder.AddCssClass("code");
            inputTextBuilder.AddCssClass($"workflow-expression-{Syntax.HtmlClassify()}");

            output.Content.Clear();
            output.Content.AppendHtml(inputTextBuilder);
            output.Content.AppendHtml($@"<div class=""input-group-append""><span class=""input-group-text"">{Syntax}</span></div>");
        }
    }
}
