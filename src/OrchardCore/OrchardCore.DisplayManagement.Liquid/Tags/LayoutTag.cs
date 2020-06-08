using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class LayoutTag : ExpressionTag
    {
        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, Expression expression)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var servicesValue))
            {
                throw new ArgumentException("Services missing while invoking 'helper'");
            }

            var services = servicesValue as IServiceProvider;

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
