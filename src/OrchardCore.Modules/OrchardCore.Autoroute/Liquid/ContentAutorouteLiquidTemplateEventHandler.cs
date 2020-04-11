using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Liquid;

namespace OrchardCore.Autoroute.Liquid
{
    public class ContentAutorouteLiquidTemplateEventHandler : ILiquidTemplateEventHandler
    {
        private readonly IContentManager _contentManager;
        private readonly IAutorouteEntries _autorouteEntries;

        public ContentAutorouteLiquidTemplateEventHandler(IContentManager contentManager, IAutorouteEntries autorouteEntries)
        {
            _contentManager = contentManager;
            _autorouteEntries = autorouteEntries;
        }

        public Task RenderingAsync(TemplateContext context)
        {
            context.MemberAccessStrategy.Register<LiquidContentAccessor, LiquidPropertyAccessor>("Slug", obj =>
            {
                return new LiquidPropertyAccessor(async alias =>
                {
                    if (!alias.StartsWith('/'))
                    {
                        alias = "/" + alias;
                    }

                    var contentItemId = await _autorouteEntries.TryGetContentItemIdAsync(alias);

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
