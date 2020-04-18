using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class HttpContextRemoveItemTag : ArgumentsTag
    {
        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] args)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var servicesValue))
            {
                throw new ArgumentException("Services missing while invoking 'helper'");
            }

            var services = servicesValue as IServiceProvider;

            var httpContext = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext;
            
            if(httpContext != null )
            {
                var arguments = (FilterArguments)(await new ArgumentsExpression(args).EvaluateAsync(context)).ToObjectValue();
                var itemKey = arguments["item"].Or(arguments.At(0)).ToStringValue();
                if (!string.IsNullOrEmpty(itemKey))
                {
                    httpContext.Items.Remove(itemKey);
                }
            
            }
            return Completion.Normal;
        }
    }
}
