using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class LayoutTag : ExpressionTag
    {
        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, Expression expression)
        {
            var name = (await expression.EvaluateAsync(context)).ToStringValue();

            if (!context.AmbientValues.TryGetValue("LiquidPage", out dynamic page))
            {
                throw new ArgumentException("LiquidPage missing while invoking 'layout'");
            }

            //May be a 'LiquidPage' or a generic 'RazorPage<>'.
            if (page != null && !string.IsNullOrWhiteSpace(name))
            {
                // So we only check if the page has a 'ViewLayout' property.
                if (((object)page).GetType().GetProperty("ViewLayout") != null)
                {
                    page.ViewLayout = name;
                }
            }

            return Completion.Normal;
        }
    }
}