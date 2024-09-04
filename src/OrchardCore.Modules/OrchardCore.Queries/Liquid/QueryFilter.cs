using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.Queries.Liquid;

public class QueryFilter : ILiquidFilter
{
    private readonly IServiceProvider _serviceProvider;

    public QueryFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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

        var queryManager = _serviceProvider.GetRequiredService<IQueryManager>();

        var result = await queryManager.ExecuteQueryAsync(query, parameters);

        return FluidValue.Create(result.Items, ctx.Options);
    }
}
