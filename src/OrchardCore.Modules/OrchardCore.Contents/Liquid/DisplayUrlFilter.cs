using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Liquid;

public class DisplayUrlFilter : ILiquidFilter
{
    private readonly AutorouteOptions _autorouteOptions;
    private readonly IContentManager _contentManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;
    private readonly IUrlHelperFactory _urlHelperFactory;

    public DisplayUrlFilter(
        IOptions<AutorouteOptions> autorouteOptions,
        IContentManager contentManager,
        IHttpContextAccessor httpContextAccessor,
        LinkGenerator linkGenerator,
        IUrlHelperFactory urlHelperFactory)
    {
        _autorouteOptions = autorouteOptions.Value;
        _contentManager = contentManager;
        _httpContextAccessor = httpContextAccessor;
        _linkGenerator = linkGenerator;
        _urlHelperFactory = urlHelperFactory;
    }

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            return new StringValue(string.Empty);
        }

        var contentItem = input.ToObjectValue() as ContentItem;
        RouteValueDictionary routeValues;

        if (contentItem == null)
        {
            if (string.IsNullOrEmpty(input.ToStringValue()))
            {
                return StringValue.Empty;
            }

            routeValues = new RouteValueDictionary(_autorouteOptions.GlobalRouteValues)
            {
                [_autorouteOptions.ContentItemIdKey] = input.ToStringValue(),
            };
        }
        else
        {
            var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);
            routeValues = contentItemMetadata.DisplayRouteValues;
        }

        var urlHelper = _urlHelperFactory.GetUrlHelper(context.ViewContext);

        var linkUrl = urlHelper.RouteUrl(routeValues);

        return new StringValue(linkUrl);
    }
}
