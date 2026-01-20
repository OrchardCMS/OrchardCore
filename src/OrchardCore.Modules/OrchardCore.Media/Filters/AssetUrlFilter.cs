using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;

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
