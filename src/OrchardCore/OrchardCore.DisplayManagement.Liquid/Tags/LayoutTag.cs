using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using OrchardCore.DisplayManagement.Razor;

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

            if (page is IRazorPage razorPage)
            {
                razorPage.ViewLayout = name;
            }

            return Completion.Normal;
        }
    }
}