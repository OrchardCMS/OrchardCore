using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class RenderSectionTag
    {
        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> argumentsList, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("ThemeLayout", out dynamic layout))
            {
                throw new ArgumentException("ThemeLayout missing while invoking 'render_section'");
            }

            if (!context.AmbientValues.TryGetValue("DisplayHelper", out var item) || !(item is IDisplayHelper displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'render_section'");
            }

            var arguments = new NamedExpressionList(argumentsList);

            var nameExpression = arguments["name", 0] ?? throw new ArgumentException("render_section tag requires a name argument");
            var name = (await nameExpression.EvaluateAsync(context)).ToStringValue();

            var requiredExpression = arguments["required", 1];
            var required = requiredExpression == null ? false : (await requiredExpression.EvaluateAsync(context)).ToBooleanValue();

            var zone = layout[name];

            if (required && zone != null && zone is Shape && zone.Items.Count == 0)
            {
                throw new InvalidOperationException("Zone not found while invoking 'render_section': " + name);
            }

            IHtmlContent htmlContent = await displayHelper.ShapeExecuteAsync(zone);
            htmlContent.WriteTo(writer, (HtmlEncoder)encoder);
            return Completion.Normal;
        }
    }
}
