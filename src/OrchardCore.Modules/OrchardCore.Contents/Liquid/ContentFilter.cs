using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Liquid
{
    public class ContentFilter : ILiquidFilter
    {
        private readonly IContentManager _contentManager;
        private readonly IContentAliasManager _contentAliasManager;

        public ContentFilter(IContentManager contentManager, IContentAliasManager contentAliasManager)
        {
            _contentManager = contentManager;
            _contentAliasManager = contentAliasManager;
        }

        public async Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var latest = arguments["latest"].ToBooleanValue();
            var mode = arguments["mode"].Or(arguments.At(0));

            if (mode.IsNil() || mode.ToStringValue() == "id")
            {
                var contentItemId = input.ToStringValue();
                var contentItem = await _contentManager.GetAsync(contentItemId, latest ? VersionOptions.Latest : VersionOptions.Published);
                return FluidValue.Create(contentItem);
            }
            if (mode.ToStringValue() == "alias")
            {
                var contentItemId = await _contentAliasManager.GetContentItemIdAsync(input.ToStringValue());

                if (contentItemId != null)
                {
                    var contentItem = await _contentManager.GetAsync(contentItemId, latest ? VersionOptions.Latest : VersionOptions.Published);
                    return FluidValue.Create(contentItem);
                }
            }
            else if (mode.ToStringValue() == "version")
            {
                var contentItemVersionId = input.ToStringValue();
                var contentItem = await _contentManager.GetVersionAsync(contentItemVersionId);
                return FluidValue.Create(contentItem);
            }

            return FluidValue.Create(null);
        }
    }
}
