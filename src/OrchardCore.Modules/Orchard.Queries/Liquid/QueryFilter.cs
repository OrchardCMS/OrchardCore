using System.Collections.Generic;
using Fluid;
using Fluid.Values;
using Orchard.Liquid;

namespace Orchard.Queries.Liquid
{
    public class QueryFilter : ITemplateContextHandler
    {
        private readonly IQueryManager _queryManager;

        public QueryFilter(IQueryManager queryManager)
        {
            _queryManager = queryManager;
        }

        public void OnTemplateProcessing(TemplateContext context)
        {
            context.Filters.AddAsyncFilter("query", async (input, arguments, ctx) =>
            {
                var query = await _queryManager.GetQueryAsync(input.ToStringValue());

                if (query == null)
                {
                    return null;
                }

                var parameters = new Dictionary<string, object>();

                foreach(var name in arguments.Names)
                {
                    parameters.Add(name, arguments[name]);
                }

                var result = await _queryManager.ExecuteQueryAsync(query, parameters);
                
                return FluidValue.Create(result);
            });
        }
    }
}
