using System;
using System.Collections.Generic;
using Fluid;
using Fluid.Values;
using Orchard.Liquid;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Queries.Liquid
{
    public class QueryFilter : ITemplateContextHandler
    {
        private IQueryManager _queryManager;
        private readonly IServiceProvider _serviceProvider;

        public QueryFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public void OnTemplateProcessing(TemplateContext context)
        {
            context.Filters.AddAsyncFilter("query", async (input, arguments, ctx) =>
            {
                _queryManager = _queryManager ?? _serviceProvider.GetService<IQueryManager>();

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
