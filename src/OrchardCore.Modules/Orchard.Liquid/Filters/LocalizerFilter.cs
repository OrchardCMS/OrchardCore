using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc.Localization;

namespace Orchard.Liquid.Filters
{
    public class LocalizerFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            object localizer;
            if (!ctx.AmbientValues.TryGetValue("ViewLocalizer", out localizer))
            {
                throw new ArgumentException("ViewLocalizer missing while invoking 't'");
            }

            var parameters = new List<object>();
            for (var i = 0; i < arguments.Count; i++)
            {
                parameters.Add(arguments.At(i).ToStringValue());
            }

            return Task.FromResult<FluidValue>(new StringValue(((IViewLocalizer)localizer).GetString(input.ToStringValue(), parameters.ToArray())));
        }
    }

 }
