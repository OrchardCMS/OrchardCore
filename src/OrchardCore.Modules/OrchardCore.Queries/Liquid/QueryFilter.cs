using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;

namespace OrchardCore.Queries.Liquid
{
    public class QueryFilter : ILiquidFilter
    {
        private readonly IQueryManager _queryManager;

        public QueryFilter(IQueryManager queryManager)
        {
            _queryManager = queryManager;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            var query = input.ToObjectValue() as Query;

            if (query == null)
            {
                return NilValue.Instance;
            }

            var parameters = new Dictionary<string, object>();

            foreach (var name in arguments.Names)
            {
                parameters.Add(name, arguments[name].ToObjectValue());
            }

            var result = await _queryManager.ExecuteQueryAsync(query, parameters);

            return FluidValue.Create(result.Items, ctx.Options);
        }
    }
}
