using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Orchard.DisplayManagement.Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class RenderSectionStatement : Statement
    {
        private readonly ArgumentsExpression _arguments;

        public RenderSectionStatement(string name, ArgumentsExpression arguments)
        {
            Name = name;
            _arguments = arguments;
        }

        public string Name { get; }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "render_section");
            var arguments = _arguments == null ? new FilterArguments()
                : (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            var required = arguments.HasNamed("required") ? Convert.ToBoolean(arguments["required"].ToStringValue()) : false;
            var htmlContent = await page.RenderSectionAsync(Name, required);
            htmlContent.WriteTo(writer, page.HtmlEncoder);
            return Completion.Normal;
        }
    }
}