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
            context.MemberAccessStrategy.Register<LiquidContentAccessor, LiquidPropertyAccessor>("Alias", obj =>
            {
                return new LiquidPropertyAccessor(async alias =>
                {
                    var contentItem = await _session.Query<ContentItem, AliasPartIndex>(x =>
                        x.Published && x.Alias == alias.ToLowerInvariant())
                        .FirstOrDefaultAsync();

                    if (contentItem == null)
                    {
                        return NilValue.Instance;
                    }

                    contentItem = await _contentManager.LoadAsync(contentItem);

                    return FluidValue.Create(contentItem);
                });
            });

            return Task.CompletedTask;
        }
    }
}
