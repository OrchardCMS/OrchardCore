using Microsoft.AspNetCore.Http;
using System.Threading;
using Microsoft.AspNetCore.Routing;
using System.Threading;
using Microsoft.Extensions.Options;
using System.Threading;
using OrchardCore.ContentManagement;
using System.Threading;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using System.Threading;
using OrchardCore.ContentManagement.Routing;
using System.Threading;
using OrchardCore.DisplayManagement;
using System.Threading;
using OrchardCore.DisplayManagement.Handlers;
using System.Threading;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Threading;
using OrchardCore.DisplayManagement.Views;
using System.Threading;
using OrchardCore.Navigation;
using System.Threading;
using OrchardCore.Taxonomies.Core;
using System.Threading;
using OrchardCore.Taxonomies.Models;
using System.Threading;
using OrchardCore.Taxonomies.ViewModels;
using System.Threading;

namespace OrchardCore.Taxonomies.Drivers;

public sealed class TermPartContentDriver : ContentDisplayDriver
{
    private readonly IContentsTaxonomyListQueryService _contentsTaxonomyListQueryService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AutorouteOptions _autorouteOptions;
    private readonly PagerOptions _pagerOptions;
    private readonly IContentManager _contentManager;

    public TermPartContentDriver(
        IOptions<PagerOptions> pagerOptions,
        IHttpContextAccessor httpContextAccessor,
        IOptions<AutorouteOptions> autorouteOptions,
        IContentsTaxonomyListQueryService contentsTaxonomyListQueryService,
        IContentManager contentManager)
    {
        _contentsTaxonomyListQueryService = contentsTaxonomyListQueryService;
        _httpContextAccessor = httpContextAccessor;
        _autorouteOptions = autorouteOptions.Value;
        _pagerOptions = pagerOptions.Value;
        _contentManager = contentManager;
    }

    public override IDisplayResult Display(ContentItem model, BuildDisplayContext context)
    {
        if (!model.TryGet<TermPart>(out var part))
        {
            return null;
        }

        return Initialize<TermPartViewModel>("TermPart", async m =>
        {
            var pager = await GetPagerAsync(context.Updater, _pagerOptions.GetPageSize());

            var (totalItemCount, containedItems) = await QueryTermItemsAsync(part, pager);
            m.TaxonomyContentItemId = part.TaxonomyContentItemId;
            m.ContentItem = part.ContentItem;
            m.ContentItems = containedItems;

            var routeValues = new RouteValueDictionary(_autorouteOptions.GlobalRouteValues);

            routeValues[_autorouteOptions.ContentItemIdKey] = part.ContentItem.ContentItemId;

            if (_httpContextAccessor.HttpContext?.Request?.RouteValues is not null &&
            _httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue(_autorouteOptions.JsonPathKey, out var jsonPath))
            {
                routeValues[_autorouteOptions.JsonPathKey] = jsonPath;
            }

            m.Pager = await context.ShapeFactory.PagerAsync(pager, totalItemCount, routeValues);
        }).Location(OrchardCoreConstants.DisplayType.Detail, "Content:5");
    }

    private async Task<(int, IEnumerable<ContentItem>)> QueryTermItemsAsync(TermPart termPart, Pager pager)
    {
        var query = await _contentsTaxonomyListQueryService.QueryAsync(termPart, pager);

        var containedItems = await query.ListAsync(cancellationToken: CancellationToken.None);

        await _contentManager.LoadAsync(containedItems);

        return (await query.CountAsync(cancellationToken: CancellationToken.None), containedItems);
    }

    private static async Task<Pager> GetPagerAsync(IUpdateModel updater, int pageSize)
    {
        var pagerParameters = new PagerParameters();

        await updater.TryUpdateModelAsync(pagerParameters);

        var pager = new Pager(pagerParameters, pageSize);

        return pager;
    }
}
