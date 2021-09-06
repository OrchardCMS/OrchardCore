using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Liquid;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Filters
{
    public static class MediaFilters
    {
        public static ValueTask<FluidValue> ImgTag(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var url = input.ToStringValue();

            var imgTag = $"<img src=\"{url}\"";

            foreach (var name in arguments.Names)
            {
                imgTag += $" {name.Replace('_', '-')}=\"{arguments[name].ToStringValue()}\"";
            }

            imgTag += " />";

            return new ValueTask<FluidValue>(new StringValue(imgTag) { Encode = false });
        }
    }
}
