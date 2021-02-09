using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.Queries.Liquid
{
    public static class QueryFilter
    {
        public static async ValueTask<FluidValue> Query(FluidValue input, FilterArguments arguments, TemplateContext ctx)
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

            var context = (LiquidTemplateContext)ctx;
            var queryManager = context.Services.GetRequiredService<IQueryManager>();

            var result = await queryManager.ExecuteQueryAsync(query, parameters);
            return FluidValue.Create(result.Items, ctx.Options);
        }
    }
}
