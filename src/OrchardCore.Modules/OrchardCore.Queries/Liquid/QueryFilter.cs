using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;

namespace OrchardCore.Queries.Liquid
{
    public class QueryFilter : ILiquidFilter
    {
        private IQueryManager _queryManager;

        public QueryFilter(IQueryManager queryManager)
        {
            _queryManager = queryManager;
        }

        public async Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var query = await _queryManager.GetQueryAsync(input.ToStringValue());

            if (query == null)
            {
                return null;
            }

            var parameters = new Dictionary<string, object>();

            foreach (var name in arguments.Names)
            {
                parameters.Add(name, arguments[name].ToObjectValue());
            }

            var result = await _queryManager.ExecuteQueryAsync(query, parameters);

            return FluidValue.Create(result);
        }
    }
}
