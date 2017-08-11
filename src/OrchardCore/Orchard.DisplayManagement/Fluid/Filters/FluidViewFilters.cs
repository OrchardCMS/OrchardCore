using System;
using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement.Fluid.Filters
{
    public static class FluidViewFilters
    {
        public static FilterCollection WithFluidViewFilters(this FilterCollection filters)
        {
            filters.AddAsyncFilter("date_time", DateTimeShape);
            return filters;
        }

        public static async Task<FluidValue> DateTimeShape(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var obj = input.ToObjectValue();
            DateTime? dateTime = null;

            if (obj is string stringDate)
            {
                var date = DateTime.Parse(stringDate, context.CultureInfo, DateTimeStyles.AssumeUniversal);
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
                var page = FluidViewTemplate.EnsureFluidPage(context, "date_time");

                Shape shape = arguments.HasNamed("format")
                    ? page.New.DateTime(Utc: dateTime, Format: arguments["format"].ToStringValue())
                    : page.New.DateTime(Utc: dateTime);

                return new StringValue((await page.DisplayAsync(shape)).ToString());
            }

            return input;
        }
    }
}