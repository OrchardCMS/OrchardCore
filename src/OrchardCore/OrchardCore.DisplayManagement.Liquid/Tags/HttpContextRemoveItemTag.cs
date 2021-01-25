using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class HttpContextRemoveItemTag
    {
        public static async ValueTask<Completion> WriteToAsync(Expression argument, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var servicesValue))
            {
                throw new ArgumentException("Services missing while invoking 'helper'");
            }

            var services = servicesValue as IServiceProvider;

            var httpContext = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext;

            if (httpContext != null)
            {
                var itemKey = (await argument.EvaluateAsync(context)).ToStringValue();

                if (!string.IsNullOrEmpty(itemKey))
                {
                    httpContext.Items.Remove(itemKey);
                }

            }
            return Completion.Normal;
        }
    }
}
