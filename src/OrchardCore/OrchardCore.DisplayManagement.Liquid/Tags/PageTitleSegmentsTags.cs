using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Tags;

internal static class PageTitleSegmentsTags
{
    public static async ValueTask<Completion> WritePageTitleAsync(IReadOnlyList<FilterArgument> argumentsList, IFluidOutput output, TextEncoder encoder, TemplateContext context)
    {
        var arguments = new NamedExpressionList(argumentsList);

        var titleBuilder = await AddSegmentInternalAsync(arguments, context);

        var separatorExpression = arguments["separator", 2];
        var separator = separatorExpression == null ? null : new HtmlString((await separatorExpression.EvaluateAsync(context)).ToStringValue(context));

        output.Write(titleBuilder.GenerateTitle(separator), (HtmlEncoder)encoder);
        return Completion.Normal;
    }

    public static async ValueTask<Completion> WriteAddSegmentAsync(IReadOnlyList<FilterArgument> argumentsList, IFluidOutput _1, TextEncoder _2, TemplateContext context)
    {
        var arguments = new NamedExpressionList(argumentsList);

        await AddSegmentInternalAsync(arguments, context);

        return Completion.Normal;
    }

    private static async Task<IPageTitleBuilder> AddSegmentInternalAsync(NamedExpressionList arguments, TemplateContext context)
    {
        var services = ((LiquidTemplateContext)context).Services;

        var titleBuilder = services.GetRequiredService<IPageTitleBuilder>();

        var segmentExpression = arguments["segment", 0] ?? throw new ArgumentException("page_title tag requires a segment argument");
        var segment = (await segmentExpression.EvaluateAsync(context)).ToStringValue(context);

        var positionExpression = arguments["position", 1];
        var position = positionExpression == null ? "0" : (await positionExpression.EvaluateAsync(context)).ToStringValue(context);

        titleBuilder.AddSegment(new HtmlString(segment), position);

        return titleBuilder;
    }
}
