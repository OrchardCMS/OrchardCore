using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class HttpContextRemoveItemTag
    {
        public static async ValueTask<Completion> WriteToAsync(Expression argument, TextWriter _1, TextEncoder _2, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;

            var httpContext = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext;

            if (httpContext != null)
            {
                var itemKey = (await argument.EvaluateAsync(context)).ToStringValue();

                if (!String.IsNullOrEmpty(itemKey))
                {
                    httpContext.Items.Remove(itemKey);
                }

            }
            return Completion.Normal;
        }
    }
}
