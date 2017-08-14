using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Irony.Parsing;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Fluid.Ast;
using Orchard.DisplayManagement.Title;

namespace Orchard.DisplayManagement.Fluid.Tags
{
    public class RenderTitleSegmentsTag : ITag
    {
        public BnfTerm GetSyntax(FluidGrammar grammar)
        {
            return grammar.FilterArguments;
        }

        public Statement Parse(ParseTreeNode node, ParserContext context)
        {
            return new RenderTitleSegmentsStatement(ArgumentsExpression.Build(node.ChildNodes[0]));
        }
    }

    public class RenderTitleSegmentsStatement : Statement
    {
        private readonly ArgumentsExpression _arguments;

        public RenderTitleSegmentsStatement(ArgumentsExpression arguments)
        {
            _arguments = arguments;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'page_title'");
            }

            var titleBuilder = ((IServiceProvider)services).GetRequiredService<IPageTitleBuilder>();

            //var segment = new HtmlString((await Segment.EvaluateAsync(context)).ToStringValue());

            var arguments = _arguments == null ? new FilterArguments()
                : (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            var segment = new HtmlString(arguments.At(0).ToStringValue());
            var position = arguments.HasNamed("position") ? arguments["position"].ToStringValue() : "0";
            var separator = arguments.HasNamed("separator") ? new HtmlString(arguments["separator"].ToStringValue()) : null;

            titleBuilder.AddSegment(segment, position);
            titleBuilder.GenerateTitle(separator).WriteTo(writer, HtmlEncoder.Default);
            return Completion.Normal;
        }
    }
}