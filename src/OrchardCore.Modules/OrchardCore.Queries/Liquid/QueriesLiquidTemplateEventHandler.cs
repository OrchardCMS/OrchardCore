using System.Threading.Tasks;
using Fluid.Values;
using OrchardCore.Liquid;

namespace OrchardCore.Queries.Liquid
{
    public class QueriesLiquidTemplateEventHandler : ILiquidTemplateEventHandler
    {
        private readonly IQueryManager _queryManager;

        public QueriesLiquidTemplateEventHandler(IQueryManager queryManager)
        {
            _queryManager = queryManager;
        }

        public Task RenderingAsync(LiquidTemplateContext context)
        {
            context.SetValue("Queries", new LiquidPropertyAccessor(async name => FluidValue.Create(await _queryManager.GetQueryAsync(name))));

            return Task.CompletedTask;
        }
    }
}
