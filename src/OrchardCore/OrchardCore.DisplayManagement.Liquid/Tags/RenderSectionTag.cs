using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Tags;

public class RenderSectionTag
{
    public static async ValueTask<Completion> WriteToAsync(IReadOnlyList<FilterArgument> argumentsList, TextWriter writer, TextEncoder encoder, TemplateContext context)
    {
        var services = ((LiquidTemplateContext)context).Services;

        var layout = await services.GetRequiredService<ILayoutAccessor>().GetLayoutAsync().ConfigureAwait(false);
        var displayHelper = services.GetRequiredService<IDisplayHelper>();

        var arguments = new NamedExpressionList(argumentsList);

        var nameExpression = arguments["name", 0] ?? throw new ArgumentException("render_section tag requires a name argument");
        var name = (await nameExpression.EvaluateAsync(context).ConfigureAwait(false)).ToStringValue();

        var requiredExpression = arguments["required", 1];
        var required = requiredExpression != null && (await requiredExpression.EvaluateAsync(context).ConfigureAwait(false)).ToBooleanValue();

        var zone = layout.Zones[name];

        if (zone.IsNullOrEmpty())
        {
            if (required)
            {
                throw new InvalidOperationException("Zone not found while invoking 'render_section': " + name);
            }

            return Completion.Normal;
        }

        var htmlContent = await displayHelper.ShapeExecuteAsync(zone).ConfigureAwait(false);
        htmlContent.WriteTo(writer, (HtmlEncoder)encoder);

        return Completion.Normal;
    }
}
