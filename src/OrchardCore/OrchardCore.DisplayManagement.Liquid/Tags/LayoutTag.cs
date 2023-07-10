using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class LayoutTag
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public static async ValueTask<Completion> WriteToAsync(Expression expression, TextWriter writer, TextEncoder encoder, TemplateContext context)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var services = ((LiquidTemplateContext)context).Services;

            var viewContextAccessor = services.GetRequiredService<ViewContextAccessor>();
            var viewContext = viewContextAccessor.ViewContext;

            if (viewContext.View is RazorView razorView && razorView.RazorPage is Razor.IRazorPage razorPage)
            {
                razorPage.ViewLayout = (await expression.EvaluateAsync(context)).ToStringValue();
            }

            return Completion.Normal;
        }
    }
}
