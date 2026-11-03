using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Tags;

public class LayoutTag
{
    public static async ValueTask<Completion> WriteToAsync(Expression expression, TextWriter _1, TextEncoder _2, TemplateContext context)
    {
        var services = ((LiquidTemplateContext)context).Services;

        var viewContextAccessor = services.GetRequiredService<ViewContextAccessor>();
        var viewContext = viewContextAccessor.ViewContext;

        if (viewContext.View is RazorView razorView && razorView.RazorPage is Razor.IRazorPage razorPage)
        {
            razorPage.ViewLayout = (await expression.EvaluateAsync(context)).ToStringValue();
        }
        else if (viewContext.HttpContext?.Features.Get<RazorViewFeature>()?.ThemeLayout is IShape layout)
        {
            // Shape templates are rendered without a Razor page, so the alternate is added to the layout of the theme
            // as the Razor page does.
            var value = (await expression.EvaluateAsync(context)).ToStringValue();

            if (layout.Metadata.Alternates.Last != value)
            {
                layout.Metadata.Alternates.Remove(value);
                layout.Metadata.Alternates.Add(value);
            }
        }

        return Completion.Normal;
    }
}
