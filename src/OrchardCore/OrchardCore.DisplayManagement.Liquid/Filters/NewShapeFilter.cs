using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Liquid.Filters
{
    public class NewShapeFilter : ILiquidFilter
    {
        private readonly IShapeFactory _shapeFactory;

        public NewShapeFilter(IShapeFactory shapeFactory)
        {
            _shapeFactory = shapeFactory;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            static async ValueTask<FluidValue> Awaited(ValueTask<IShape> task, TemplateOptions options)
            {
                return FluidValue.Create(await task, options);
            }

            var type = input.ToStringValue();
            var properties = new Dictionary<string, object>(arguments.Count);

            foreach (var name in arguments.Names)
            {
                properties.Add(name.ToPascalCaseUnderscore(), arguments[name].ToObjectValue());
            }

            var task = _shapeFactory.CreateAsync(type, Arguments.From(properties));
            if (!task.IsCompletedSuccessfully)
            {
                return Awaited(task, ctx.Options);
            }

            return new ValueTask<FluidValue>(FluidValue.Create(task.Result, ctx.Options));
        }
    }
}
