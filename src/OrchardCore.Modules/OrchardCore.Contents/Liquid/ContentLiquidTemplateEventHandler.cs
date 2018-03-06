using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Liquid
{
    public class ContentLiquidTemplateEventHandler : ILiquidTemplateEventHandler
    {
        private readonly IContentManager _contentManager;
        private readonly IContentAliasManager _contentAliasManager;

        public ContentLiquidTemplateEventHandler(IContentManager contentManager, IContentAliasManager contentAliasManager)
        {
            _contentManager = contentManager;
            _contentAliasManager = contentAliasManager;
        }

        public Task RenderingAsync(TemplateContext context)
        {
            context.LocalScope.SetValue("Content", new LiquidContentAccessor());
            // TODO: change to async/await once https://github.com/sebastienros/fluid/pull/67 is merged
            context.MemberAccessStrategy.Register<LiquidPropertyAccessor, FluidValue>((obj, name) => obj.GetValueAsync(name));

            context.MemberAccessStrategy.Register<LiquidContentAccessor, LiquidPropertyAccessor>("ContentItemId", obj =>
            {
                return new LiquidPropertyAccessor(async contentItemId => FluidValue.Create(await _contentManager.GetAsync(contentItemId)));
            });

            context.MemberAccessStrategy.Register<LiquidContentAccessor, LiquidPropertyAccessor>("ContentItemVersionId", obj =>
            {
                return new LiquidPropertyAccessor(async contentItemVersionId => FluidValue.Create(await _contentManager.GetVersionAsync(contentItemVersionId)));
            });

            context.MemberAccessStrategy.Register<LiquidContentAccessor, LiquidPropertyAccessor>("Latest", obj => new LiquidPropertyAccessor(name => GetContentByAlias(name, true)));

            // TODO: change to async/await once https://github.com/sebastienros/fluid/pull/67 is merged
            context.MemberAccessStrategy.Register<LiquidContentAccessor, FluidValue>((obj, name) => GetContentByAlias(name));

            return Task.CompletedTask;
        }

        private async Task<FluidValue> GetContentByAlias(string alias, bool latest = false)
        {
            var contentItemId = await _contentAliasManager.GetContentItemIdAsync(alias);

            if (contentItemId == null)
            {
                return NilValue.Instance;
            }

            var contentItem = await _contentManager.GetAsync(contentItemId, latest ? VersionOptions.Latest : VersionOptions.Published);
            return FluidValue.Create(contentItem);
        }
    }
}