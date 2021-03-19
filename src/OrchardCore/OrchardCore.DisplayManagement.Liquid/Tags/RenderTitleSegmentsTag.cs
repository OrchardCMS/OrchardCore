using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class RenderTitleSegmentsTag
    {
        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;

            var arguments = new NamedExpressionList(argumentsList);

            var titleBuilder = services.GetRequiredService<IPageTitleBuilder>();

            var segmentExpression = arguments["segment", 0] ?? throw new ArgumentException("page_title tag requires a segment argument");
            var segment = (await segmentExpression.EvaluateAsync(context)).ToStringValue();

            var positionExpression = arguments["position", 1];
            var position = positionExpression == null ? "0" : (await positionExpression.EvaluateAsync(context)).ToStringValue();

            var separatorExpression = arguments["separator", 2];
            var separator = separatorExpression == null ? null : new HtmlString((await separatorExpression.EvaluateAsync(context)).ToStringValue());

            titleBuilder.AddSegment(new HtmlString(segment), position);
            titleBuilder.GenerateTitle(separator).WriteTo(writer, (HtmlEncoder)encoder);
            return Completion.Normal;
        }
    }
}
