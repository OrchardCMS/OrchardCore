using System.Threading.Tasks;
using Fluid;
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

        public Task RenderingAsync(TemplateContext context)
        {
            context.LocalScope.SetValue("Queries", new LiquidPropertyAccessor(async name => FluidValue.Create(await _queryManager.GetQueryAsync(name))));

            return Task.CompletedTask;
        }
    }
}