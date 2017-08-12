using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Html;
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
            if (!context.AmbientValues.TryGetValue("ThemeLayout", out dynamic layout))
            {
                throw new ArgumentException("ThemeLayout missing while invoking 'render_section'");
            }

            if (!context.AmbientValues.TryGetValue("DisplayHelper", out dynamic displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'render_section'");
            }

            var arguments = _arguments == null ? new FilterArguments()
                : (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            var required = arguments.HasNamed("required") ? Convert.ToBoolean(arguments["required"].ToStringValue()) : false;

            var zone = layout[Name];

            if (required && zone != null && zone.Items.Count == 0)
            {
                throw new InvalidOperationException("Zone not found while invoking 'render_section': " + Name);
            }

            var htmlContent = await (Task<IHtmlContent>)displayHelper(zone);
            htmlContent.WriteTo(writer, HtmlEncoder.Default);
            return Completion.Normal;
        }
    }
}