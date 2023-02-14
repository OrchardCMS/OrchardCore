using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement;
using OrchardCore.Entities;
using OrchardCore.Navigation;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Abstractions.ViewModels;
using OrchardCore.Search.Model;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Search;

public class SearchController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ISiteService _siteService;
    private readonly ISession _session;
    private readonly IStringLocalizer S;
    private readonly IServiceProvider _serviceProvider;
    private readonly IShapeFactory _shapeFactory;
    private readonly dynamic New;
    private readonly ILogger _logger;

    public SearchController(
        IAuthorizationService authorizationService,
        ISiteService siteService,
        ISession session,
        IStringLocalizer<SearchController> stringLocalizer,
        IEnumerable<IPermissionProvider> permissionProviders,
        IServiceProvider serviceProvider,
        IShapeFactory shapeFactory,
        ILogger<SearchController> logger
        )
    {
        _authorizationService = authorizationService;
        _siteService = siteService;
        _session = session;
        S = stringLocalizer;
        _serviceProvider = serviceProvider;
        _shapeFactory = shapeFactory;
        New = shapeFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Search(string index, SearchIndexViewModel viewModel, PagerSlimParameters pagerParameters)
    {
        if (!String.IsNullOrEmpty(index))
        {
            viewModel.Index = index;
        }

        var searchServices = _serviceProvider.GetServices<ISearchService>();

        if (!searchServices.Any())
        {
            _logger.LogInformation("No search provider feature is enabled.");

            return BadRequest("Missing search provider service.");
        }

        var site = await _siteService.GetSiteSettingsAsync();

        var searchSettings = site.As<SearchSettings>();

        var searchService = GetSearchService(searchServices, searchSettings?.SearchProviderAreaName);

        var indexName = !String.IsNullOrWhiteSpace(viewModel.Index) ? viewModel.Index.Trim() : await searchService.DefaultIndexAsync();

        if (indexName == null || !await searchService.ExistsAsync(indexName))
        {
            _logger.LogInformation("Couldn't execute search. The search index doesn't exist.");

            return BadRequest("Search provider is not configured.");
        }

        var permission = await searchService.GetPermissionAsync(indexName);

        if (permission != null && !await _authorizationService.AuthorizeAsync(User, permission))
        {
            return this.ChallengeOrForbid();
        }

        var defaultSearchFields = await searchService.GetSearchFieldsAsync(indexName);

        if (defaultSearchFields == null || defaultSearchFields.Length == 0)
        {
            _logger.LogInformation("Couldn't execute search. No serach provider settings was defined.");

            return BadRequest("Search provider is not configured.");
        }

        if (String.IsNullOrWhiteSpace(viewModel.Terms))
        {
            return View(await GetEmptyShape(viewModel, searchSettings));
        }

        var siteSettings = await _siteService.GetSiteSettingsAsync();

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

        var searchResult = await searchService.GetAsync(indexName, viewModel.Terms, defaultSearchFields, from, size);

        if (!searchResult.Success || !searchResult.ContentItemIds.Any())
        {
            return View(await GetEmptyShape(viewModel, searchSettings));
        }

        // Query database to retrieve content items.
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

        var shape = await _shapeFactory.CreateAsync<SearchIndexViewModel>("SearchList", async model =>
        {
            model.PageTitle = searchSettings.PageTitle;
            model.Terms = viewModel.Terms;
            model.SearchForm = new SearchFormViewModel("Search__Form")
            {
                Terms = viewModel.Terms,
                Placeholder = searchSettings.Placeholder,
                Index = viewModel.Index,
            };
            model.SearchResults = new SearchResultsViewModel("Search__Results")
            {
                ContentItems = containedItems.OrderBy(x => searchResult.ContentItemIds.IndexOf(x.ContentItemId))
                .Take(pager.PageSize)
                .ToList(),
            };
            model.Pager = (await New.PagerSlim(pager)).UrlParams(new Dictionary<string, string>()
            {
                { "Terms", viewModel.Terms },
                { "Index", viewModel.Index },
            });
        });

        return View(shape);
    }

    private async Task<IShape> GetEmptyShape(SearchIndexViewModel viewModel, SearchSettings searchSettings)
    {
        return await _shapeFactory.CreateAsync<SearchIndexViewModel>("SearchList", model =>
        {
            model.PageTitle = searchSettings.PageTitle;
            model.Index = viewModel.Index;
            model.SearchForm = new SearchFormViewModel("Search__Form")
            {
                Terms = viewModel.Terms,
                Placeholder = searchSettings.Placeholder,
                Index = viewModel.Index,
            };
        });
    }

    private ISearchService GetSearchService(IEnumerable<ISearchService> searchServices, string providerName)
    {
        ISearchService searchService = null;

        if (!String.IsNullOrEmpty(providerName))
        {
            var searchProvider = _serviceProvider.GetServices<SearchProvider>()
                .FirstOrDefault(x => x.AreaName == providerName);

            if (searchProvider != null)
            {
                searchService = searchServices.FirstOrDefault(service => service.CanHandle(searchProvider));
            }
        }

        return searchService ?? searchServices.First();
    }
}
