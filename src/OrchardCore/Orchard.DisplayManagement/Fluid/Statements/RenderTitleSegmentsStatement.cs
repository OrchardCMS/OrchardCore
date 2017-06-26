using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Html;
using Orchard.DisplayManagement.Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class RenderTitleSegmentsStatement : Statement
    {
        private readonly ArgumentsExpression _arguments;

        public RenderTitleSegmentsStatement(ArgumentsExpression arguments)
        {
            _arguments = arguments;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (context.AmbientValues.TryGetValue("FluidView", out var view) && view is FluidView)
            {
                var arguments = (await _arguments.EvaluateAsync(context)).ToObjectValue() as FilterArguments;

                var segment = new HtmlString(arguments.At(0).ToStringValue());

                var position = arguments.HasNamed("position") ?
                    arguments["position"].ToStringValue() : "0";

                var separator = arguments.HasNamed("separator") ?
                    new HtmlString(arguments["separator"].ToStringValue()) : null;

                (view as FluidView).RenderTitleSegments(segment, position, separator)
                    .WriteTo(writer, HtmlEncoder.Default);
            }
            else
            {
                throw new ParseException("FluidView missing while invoking 'RenderTitleSegments'.");
            }

            return Completion.Normal;
        }
    }
}