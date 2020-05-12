using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Infrastructure.SafeCodeFilters;

namespace OrchardCore.Liquid.Filters
{
    public class SafeCodeFilter : ILiquidFilter
    {
        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'safe_code'");
            }

            var safeCodeFilterManager = ((IServiceProvider)services).GetRequiredService<ISafeCodeFilterManager>();

            return new StringValue(await safeCodeFilterManager.ProcessAsync(input.ToStringValue()));
        }
    }
}
