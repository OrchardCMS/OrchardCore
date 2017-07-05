using System;
using Fluid;
using Fluid.Values;
using Orchard.ContentManagement;
using System.Threading.Tasks;

namespace Orchard.Liquid.Filters
{
    public class DateFilter : ILiquidFilter
    {
        private readonly IContentManager _contentManager;

        public DateFilter(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var format = arguments.At(0).ToStringValue();

            switch (input.ToObjectValue())
            {
                case DateTime dateTime:
                    return Task.FromResult<FluidValue>(new StringValue(dateTime.ToString(format)));

                case DateTimeOffset dateTimeOffset:
                    return Task.FromResult<FluidValue>(new StringValue(dateTimeOffset.ToString(format)));

                default:
                    return Task.FromResult<FluidValue>(NilValue.Instance);
            }
        }
    }
}
