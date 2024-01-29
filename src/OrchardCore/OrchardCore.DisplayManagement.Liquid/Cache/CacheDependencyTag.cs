using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Cache;
using OrchardCore.Liquid;

namespace OrchardCore.DynamicCache.Liquid
{
    public class CacheDependencyTag
    {
        public static async ValueTask<Completion> WriteToAsync(Expression argument, TextWriter _1, TextEncoder _2, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;

            var cacheScopeManager = services.GetService<ICacheScopeManager>();

            if (cacheScopeManager == null)
            {
                return Completion.Normal;
            }

            var dependency = (await argument.EvaluateAsync(context)).ToStringValue();

            cacheScopeManager.AddDependencies(dependency);

            return Completion.Normal;
        }
    }
}
