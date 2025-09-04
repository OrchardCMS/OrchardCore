using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc.Routing;
using OrchardCore.Mvc.Core.Utilities;

namespace OrchardCore.Liquid.Filters;

public class AbsoluteUrlFilter : ILiquidFilter
{
    private readonly IUrlHelperFactory _urlHelperFactory;

    public AbsoluteUrlFilter(IUrlHelperFactory urlHelperFactory)
    {
        _urlHelperFactory = urlHelperFactory;
    }
    public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        var relativePath = input.ToStringValue();

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return ValueTask.FromResult(input);
        }

        var urlHelper = _urlHelperFactory.GetUrlHelper(context.ViewContext);

        var result = StringValue.Create(urlHelper.ToAbsoluteUrl(relativePath));

        return ValueTask.FromResult<FluidValue>(result);
    }
}
