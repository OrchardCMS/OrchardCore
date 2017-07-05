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

        public RenderTitleSegmentsStatement(Expression segment, ArgumentsExpression arguments)
        {
            Segment = segment;
            _arguments = arguments;
        }

        public Expression Segment { get; }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "RenderTitleSegments");

            var segment = new HtmlString((await Segment.EvaluateAsync(context)).ToStringValue());

            var arguments = _arguments == null ? new FilterArguments()
                : (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            var position = arguments.HasNamed("position") ? arguments["position"].ToStringValue() : "0";
            var separator = arguments.HasNamed("separator") ? new HtmlString(arguments["separator"].ToStringValue()) : null;

            page.RenderTitleSegments(segment, position, separator).WriteTo(writer, page.HtmlEncoder);
            return Completion.Normal;
        }
    }
}