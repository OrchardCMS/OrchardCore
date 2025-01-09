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

        var trimmedInputString = input.ToStringValue().Trim();
        var absolutePath = urlHelper.Content(trimmedInputString);

        return new StringValue(absolutePath);
    }
}
