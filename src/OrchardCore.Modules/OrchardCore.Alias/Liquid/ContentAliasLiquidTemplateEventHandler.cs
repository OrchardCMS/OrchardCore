using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Alias.Indexes;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using YesSql;

namespace OrchardCore.Alias.Liquid
{
    public class ContentAliasLiquidTemplateEventHandler : ILiquidTemplateEventHandler
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        public ContentAliasLiquidTemplateEventHandler(IContentManager contentManager, ISession session)
        {
            _contentManager = contentManager;
            _session = session;
        }

        public Task RenderingAsync(TemplateContext context)
        {
            context.MemberAccessStrategy.Register<LiquidContentAccessor>("Alias", obj =>
            {
                return new LiquidPropertyAccessor(async alias =>
                {
                    var aliasPartIndex = _session.Query<ContentItem, AliasPartIndex>(x => x.Alias == alias.ToLowerInvariant()).FirstOrDefaultAsync().GetAwaiter().GetResult();
                    var contentItemId = aliasPartIndex?.ContentItemId;

                    if (contentItemId == null)
                    {
                        return NilValue.Instance;
                    }

                    return FluidValue.Create(await _contentManager.GetAsync(contentItemId));
                });
            });

            return Task.CompletedTask;
        }
    }
}
