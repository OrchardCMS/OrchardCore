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
    public class RenderTitleSegmentsStatement : Statement
    {
        private readonly FilterArgumentsExpression _arguments;

        public RenderTitleSegmentsStatement(FilterArgumentsExpression arguments)
        {
            _arguments = arguments;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (context.AmbientValues.TryGetValue("FluidView", out var view) && view is FluidView)
            {
                var arguments = (await _arguments.EvaluateAsync(context)).ToObjectValue() as FilterArguments;

                var segment = arguments.HasNamed("segment") ? arguments["segment"].ToStringValue() : String.Empty;
                var position = arguments.HasNamed("position") ? arguments["position"].ToStringValue() : "0";
                var separator = arguments.HasNamed("separator") ? arguments["separator"].ToStringValue() : String.Empty;

                (view as FluidView).RenderTitleSegments(new HtmlString(segment), position,
                    new HtmlString(separator)).WriteTo(writer, HtmlEncoder.Default);
            }
            else
            {
                throw new ParseException("FluidView missing while invoking 'render_title_segments'.");
            }

            return Completion.Normal;
        }
    }
}