using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.Liquid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class RenderTitleSegmentsTag : ArgumentsTag
    {
        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] args)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'page_title'");
            }

            var titleBuilder = ((IServiceProvider)services).GetRequiredService<IPageTitleBuilder>();
            var arguments = (FilterArguments)(await new ArgumentsExpression(args).EvaluateAsync(context)).ToObjectValue();

            var segment = new HtmlString(arguments["segment"].Or(arguments.At(0)).ToStringValue());
            var position = arguments.HasNamed("position") ? arguments["position"].ToStringValue() : "0";
            var separator = arguments.HasNamed("separator") ? new HtmlString(arguments["separator"].ToStringValue()) : null;

            titleBuilder.AddSegment(segment, position);
            titleBuilder.GenerateTitle(separator).WriteTo(writer, HtmlEncoder.Default);
            return Completion.Normal;
        }
    }
}