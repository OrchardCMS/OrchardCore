using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Models;
using OrchardCore.Search.ViewModels;
using OrchardCore.Settings;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Search;

public sealed class SearchController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ISiteService _siteService;
    private readonly ISession _session;
    private readonly IServiceProvider _serviceProvider;
    private readonly INotifier _notifier;
    private readonly IEnumerable<ISearchHandler> _searchHandlers;
    private readonly IShapeFactory _shapeFactory;
    private readonly ILogger _logger;
    private readonly IIndexProfileStore _indexProfileStore;

    internal readonly IHtmlLocalizer H;

    public SearchController(
        IAuthorizationService authorizationService,
        ISiteService siteService,
        ISession session,
        IServiceProvider serviceProvider,
        INotifier notifier,
        IHtmlLocalizer<SearchController> htmlLocalizer,
        IIndexProfileStore indexProfileStore,
        IEnumerable<ISearchHandler> searchHandlers,
        IShapeFactory shapeFactory,
        ILogger<SearchController> logger
        )
    {
        _authorizationService = authorizationService;
        _siteService = siteService;
        _session = session;
        _serviceProvider = serviceProvider;
        _notifier = notifier;
        H = htmlLocalizer;
        _indexProfileStore = indexProfileStore;
        _searchHandlers = searchHandlers;
        _shapeFactory = shapeFactory;
        _logger = logger;
    }

    [Route("search/{indexName?}")]
    public async Task<IActionResult> Search(string indexName, string terms, PagerSlimParameters pagerParameters)
    {
        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var searchSettings = siteSettings.As<SearchSettings>();

        IndexProfile index = null;

        var hasIndexName = !string.IsNullOrWhiteSpace(indexName);

        if (!hasIndexName)
        {
            // Try to find the default index configured in site search settings.
            if (!string.IsNullOrEmpty(searchSettings.DefaultIndexProfileId))
            {
                index = await _indexProfileStore.FindByIdAsync(searchSettings.DefaultIndexProfileId);
            }

            if (index is null)
            {
                await _notifier.WarningAsync(H["No default search index has been configured."]);

                return View();
            }
        }
        else
        {
            index = await _indexProfileStore.FindByNameAsync(indexName);
        }

        if (index is null)
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.QuerySearchIndex, index))
        {
            return this.ChallengeOrForbid();
        }

        var searchService = _serviceProvider.GetKeyedService<ISearchService>(index.ProviderName);

        if (searchService is null)
        {
            await _notifier.WarningAsync(H["No search service provider found for {0} provider.", index.ProviderName]);

            return View();
        }

        if (string.IsNullOrWhiteSpace(terms))
        {
            return View(new SearchIndexViewModel()
            {
                IndexName = index.Name,
                PageTitle = searchSettings.PageTitle,
                SearchForm = new SearchFormViewModel()
                {
                    Terms = terms,
                    Placeholder = searchSettings.Placeholder,
                    IndexName = index.Name,
                },
            });
        }

        var pager = new PagerSlim(pagerParameters, siteSettings.PageSize);

        // Fetch one more result than PageSize to generate "More" links.
        var from = 0;
        var size = pager.PageSize + 1;

        if (pagerParameters.Before != null)
        {
            from = Convert.ToInt32(pagerParameters.Before) - pager.PageSize - 1;
            size = Convert.ToInt32(pagerParameters.Before);
        }
        else if (pagerParameters.After != null)
        {
            from = Convert.ToInt32(pagerParameters.After);
            size = Convert.ToInt32(pagerParameters.After) + pager.PageSize + 1;
        }

        var searchResult = await searchService.SearchAsync(index, terms, from, size);

        var searchContext = new SearchContext
        {
            Index = index,
            Terms = terms,
            ContentItemIds = searchResult.ContentItemIds ?? [],
            SearchService = searchService,
            TotalHits = searchResult.ContentItemIds?.Count ?? 0,
        };

        if (!searchResult.Success || searchResult.ContentItemIds.Count == 0)
        {
            await _searchHandlers.InvokeAsync((handler, context) => handler.SearchedAsync(context), searchContext, _logger);

            return View(new SearchIndexViewModel()
            {
                IndexName = index.Name,
                PageTitle = searchSettings.PageTitle,
                SearchForm = new SearchFormViewModel()
                {
                    Terms = terms,
                    Placeholder = searchSettings.Placeholder,
                    IndexName = index.Name,
                },
                SearchResults = new SearchResultsViewModel()
                {
                    IndexName = index.Name,
                    ContentItems = [],
                },
            });
        }

        // Query the database to retrieve content items.
        IQuery<ContentItem> query;

        if (searchResult.Latest)
        {
            query = _session.Query<ContentItem, ContentItemIndex>()
                .Where(x => x.ContentItemId.IsIn(searchResult.ContentItemIds) && x.Latest);
        }
        else
        {
            query = _session.Query<ContentItem, ContentItemIndex>()
                .Where(x => x.ContentItemId.IsIn(searchResult.ContentItemIds) && x.Published);
        }

        await _searchHandlers.InvokeAsync((handler, context) => handler.SearchedAsync(context), searchContext, _logger);

        // Sort the content items by their position in the search results returned by search service.
        var containedItems = await query.Take(pager.PageSize + 1).ListAsync();

        // Set the PagerSlim before and after links.
        if (pagerParameters.After != null || pagerParameters.Before != null)
        {
            pager.Before = null;

            if (from + 1 > 1)
            {
                pager.Before = (from + 1).ToString();
            }
        }

        pager.After = null;

        if (containedItems.Count() == pager.PageSize + 1)
        {
            pager.After = (size - 1).ToString();
        }

        var shape = new SearchIndexViewModel()
        {
            IndexName = index.Name,
            PageTitle = searchSettings.PageTitle,
            Terms = terms,
            SearchForm = new SearchFormViewModel()
            {
                Terms = terms,
                Placeholder = searchSettings.Placeholder,
                IndexName = index.Name,
            },
            SearchResults = new SearchResultsViewModel()
            {
                IndexName = index.Name,
                ContentItems = containedItems.OrderBy(x => searchResult.ContentItemIds.IndexOf(x.ContentItemId))
                .Take(pager.PageSize)
                .ToList(),
                Highlights = searchResult.Highlights,
            },
            Pager = await _shapeFactory.PagerSlimAsync(pager, new Dictionary<string, string>()
            {
                { nameof(terms), terms },
                { nameof(indexName), index.Name },
            }),
        };

        return View(shape);
    }
}
