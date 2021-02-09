using System;
using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Liquid.Filters
{
    public static class TimeZoneFilter
    {
        public static async ValueTask<FluidValue> Local(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var orchardContext = (LiquidTemplateContext)context;
            var localClock = orchardContext.Services.GetRequiredService<ILocalClock>();

            var value = DateTimeOffset.MinValue;

            if (input.Type == FluidValues.String)
            {
                var stringValue = input.ToStringValue();

                if (stringValue == "now" || stringValue == "today")
                {
                    value = await localClock.LocalNowAsync;
                }
                else
                {
                    if (!DateTimeOffset.TryParse(stringValue, context.Options.CultureInfo, DateTimeStyles.AssumeUniversal, out value))
                    {
                        return NilValue.Instance;
                    }
                }
            }
            else
            {
                switch (input.ToObjectValue())
                {
                    case DateTime dateTime:
                        value = dateTime;
                        break;

                    case DateTimeOffset dateTimeOffset:
                        value = dateTimeOffset;
                        break;

                    default:
                        return NilValue.Instance;
                }
            }

            return new ObjectValue(await localClock.ConvertToLocalAsync(value));
        }
    }
}
