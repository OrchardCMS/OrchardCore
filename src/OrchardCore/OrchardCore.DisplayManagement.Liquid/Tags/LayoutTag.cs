using System;
using System.IO;
//using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
//using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class LayoutTag : ExpressionTag
    {
        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, Expression expression)
        {
            //if (!context.AmbientValues.TryGetValue("ThemeLayout", out dynamic themeLayout))
            //{
            //    throw new ArgumentException("ThemeLayout missing while invoking 'layout'");
            //}

            var name = (await expression.EvaluateAsync(context)).ToStringValue();

            //if (themeLayout is IShape layout && !string.IsNullOrWhiteSpace(name))
            //{
            //    var alternates = layout.Metadata.Alternates.ToList();

            //    alternates.Insert(0, name);

            //    layout.Metadata.Alternates = new AlternatesCollection(alternates.ToArray());
            //}

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