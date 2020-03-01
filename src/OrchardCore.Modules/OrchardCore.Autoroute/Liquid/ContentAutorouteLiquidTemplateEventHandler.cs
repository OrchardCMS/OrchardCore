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

                    if (_autorouteEntries.TryGetEntryByPath(alias, out var entry))
                    {
                        // TODO this requires more work, to support contained content items.
                        // as it will require returning the id and jsonPath.
                        return FluidValue.Create(await _contentManager.GetAsync(entry.ContentItemId));
                    }

                    return NilValue.Instance;
                });
            });

            return Task.CompletedTask;
        }
    }
}
