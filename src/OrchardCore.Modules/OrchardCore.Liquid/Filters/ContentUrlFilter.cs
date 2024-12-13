using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc.Routing;

namespace OrchardCore.Liquid.Filters;

public class ContentUrlFilter : ILiquidFilter
{
    private readonly IUrlHelperFactory _urlHelperFactory;

    public ContentUrlFilter(IUrlHelperFactory urlHelperFactory)
    {
        _urlHelperFactory = urlHelperFactory;
    }

    public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        var urlHelper = _urlHelperFactory.GetUrlHelper(context.ViewContext);

        return ValueTask.FromResult<FluidValue>(new StringValue((urlHelper).Content(input.ToStringValue())));
    }
}
