using System.Collections.Generic;
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
    public class HttpContextAddItemTag
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> expressions, TextWriter writer, TextEncoder encoder, TemplateContext context)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var services = ((LiquidTemplateContext)context).Services;

            var httpContext = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext;

            if (httpContext != null)
            {
                foreach (var argument in expressions)
                {
                    httpContext.Items[argument.Name] = (await argument.Expression.EvaluateAsync(context)).ToObjectValue();
                }
            }
            return Completion.Normal;
        }
    }
}
