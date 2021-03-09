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
    public class AssetUrlFilter : ILiquidFilter
    {
        private readonly IMediaFileStore _mediaFileStore;

        public AssetUrlFilter(IMediaFileStore mediaFileStore)
        {
            _mediaFileStore = mediaFileStore;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
        {
            var url = input.ToStringValue();
            var imageUrl = _mediaFileStore.MapPathToPublicUrl(url);
            return new ValueTask<FluidValue>(new StringValue(imageUrl ?? url));
        }
    }
}
