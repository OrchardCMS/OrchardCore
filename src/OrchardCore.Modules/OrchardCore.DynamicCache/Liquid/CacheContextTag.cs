using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Cache;
using OrchardCore.Liquid.Ast;

namespace OrchardCore.DynamicCache.Liquid
{
    public class CacheContextTag : ExpressionArgumentsTag
    {
        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, Expression expression, FilterArgument[] args)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var servicesObj))
            {
                throw new ArgumentException("Services missing while invoking 'cache_context' tag");
            }

            var services = servicesObj as IServiceProvider;

            var cacheScopeManager = services.GetService<ICacheScopeManager>();

            if (cacheScopeManager == null)
            {
                return Completion.Normal;
            }

            var contextToAdd = (await expression.EvaluateAsync(context)).ToStringValue();

            cacheScopeManager.AddContexts(contextToAdd);

            return Completion.Normal;
        }
    }
}