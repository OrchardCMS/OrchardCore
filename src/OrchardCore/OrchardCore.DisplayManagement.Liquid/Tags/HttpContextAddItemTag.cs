using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class HttpContextAddItemTag
    {
        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> expressions, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var servicesValue))
            {
                throw new ArgumentException("Services missing while invoking 'helper'");
            }

            var services = servicesValue as IServiceProvider;

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
