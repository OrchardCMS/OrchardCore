using System;
using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.Liquid.Filters
{
    public class DateTimeFilter : ILiquidFilter
    {
        public async Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("ShapeFactory", out dynamic shapeFactory))
            {
                throw new ArgumentException("ShapeFactory missing while invoking 'date_time'");
            }

            if (!ctx.AmbientValues.TryGetValue("DisplayHelper", out dynamic displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'date_time'");
            }

            var obj = input.ToObjectValue();
            DateTime? dateTime = null;

            if (obj is string stringDate)
            {
                var date = DateTime.Parse(stringDate, ctx.CultureInfo, DateTimeStyles.AssumeUniversal);
                dateTime = new DateTime?(date);
            }
            else if (obj is DateTime date)
            {
                dateTime = new DateTime?(date);
            }
            else if (obj is DateTimeOffset dateTimeOffset)
            {
                dateTime = new DateTime?(dateTimeOffset.Date);
            }
            if (dateTime.HasValue)
            {
                Shape shape = arguments.HasNamed("format")
                    ? shapeFactory.DateTime(Utc: dateTime, Format: arguments["format"].ToStringValue())
                    : shapeFactory.DateTime(Utc: dateTime);

                return new StringValue((await (Task<IHtmlContent>)displayHelper(shape)).ToString());
            }

            return input;
        }
    }

    public class ShapeNamedFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var shape = input.ToObjectValue() as Shape;
            return Task.FromResult<FluidValue>(new ObjectValue(shape?.Named(arguments.At(0).ToStringValue())));
        }
    }

    public class ShapeStringFilter : ILiquidFilter
    {
        public async Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("DisplayHelper", out dynamic displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'shape_string'");
            }

            var shape = input.ToObjectValue() as IShape;
            return new StringValue((await (Task<IHtmlContent>)displayHelper(shape)).ToString());
        }
    }

    public class ClearAlternatesFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var shape = input.ToObjectValue() as IShape;

            if (shape?.Metadata.Alternates.Count > 0)
            {
                shape.Metadata.Alternates.Clear();
            }

            return Task.FromResult(input);
        }
    }

    public class ShapeTypeFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var shape = input.ToObjectValue() as IShape;

            if (shape?.Metadata != null)
            {
                shape.Metadata.Type = arguments.At(0).ToStringValue();
            }

            return Task.FromResult(input);
        }
    }

    public class DisplayTypeFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var shape = input.ToObjectValue() as IShape;

            if (shape?.Metadata != null)
            {
                shape.Metadata.DisplayType = arguments.At(0).ToStringValue();
            }

            return Task.FromResult(input);
        }
    }

    public class ShapePositionFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var shape = input.ToObjectValue() as IShape;

            if (shape?.Metadata != null)
            {
                shape.Metadata.Position = arguments.At(0).ToStringValue();
            }

            return Task.FromResult(input);
        }
    }

    public class ShapeTabFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var shape = input.ToObjectValue() as IShape;

            if (shape?.Metadata != null)
            {
                shape.Metadata.Tab = arguments.At(0).ToStringValue();
            }

            return Task.FromResult(input);
        }
    }

    public class RemoveItemFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var shape = input.ToObjectValue() as Shape;

            if (shape?.Items != null)
            {
                shape.Remove(arguments.At(0).ToStringValue());
            }

            return Task.FromResult(input);
        }
    }
}
