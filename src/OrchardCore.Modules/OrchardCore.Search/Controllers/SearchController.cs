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
    private readonly IEnumerable<IPermissionProvider> _permissionProviders;
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
        _permissionProviders = permissionProviders;
        _serviceProvider = serviceProvider;
        _shapeFactory = shapeFactory;
        New = shapeFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Search(SearchIndexViewModel viewModel, PagerSlimParameters pagerParameters)
    {
        var searchServices = _serviceProvider.GetServices<ISearchService>();

        var totalServices = searchServices.Count();

        if (totalServices == 0)
        {
            _logger.LogInformation("No search provider feature is enabled.");

            return BadRequest("Missing search provider service.");
        }

        var searchService = await GetSearchServiceAsync(searchServices, totalServices);

        var indexName = !String.IsNullOrWhiteSpace(viewModel.Index) ? viewModel.Index : await searchService.DefaultIndexAsync();

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
            var shape = await _shapeFactory.CreateAsync<SearchIndexViewModel>("SearchList", viewModel =>
            {
                viewModel.Index = viewModel.Index;
                viewModel.SearchForm = new SearchFormViewModel("Search__Form");
            });

            return View(shape);
        }

        var siteSettings = await _siteService.GetSiteSettingsAsync();

        var pager = new PagerSlim(pagerParameters, siteSettings.PageSize);

        // Fetch one more result than PageSize to generate "More" links
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
            var shape = await _shapeFactory.CreateAsync<SearchIndexViewModel>("SearchList", viewModel =>
            {
                viewModel.Index = viewModel.Index;
                viewModel.SearchForm = new SearchFormViewModel("Search__Form")
                {
                    Terms = viewModel.Terms
                };
            });

            return View(shape);
        }

        // We Query database to retrieve content items.
        IQuery<ContentItem> queryDb;

        if (searchResult.Latest)
        {
            queryDb = _session.Query<ContentItem, ContentItemIndex>()
                .Where(x => x.ContentItemId.IsIn(searchResult.ContentItemIds) && x.Latest);
        }
        else
        {
            queryDb = _session.Query<ContentItem, ContentItemIndex>()
                .Where(x => x.ContentItemId.IsIn(searchResult.ContentItemIds) && x.Published);
        }

        // Sort the content items by their rank in the search results returned by Elasticsearch.
        var containedItems = await queryDb.Take(pager.PageSize + 1).ListAsync();

        // We set the PagerSlim before and after links
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

        var shapeViewModel = await _shapeFactory.CreateAsync<SearchIndexViewModel>("SearchList", async viewModel =>
        {
            viewModel.Terms = viewModel.Terms;
            viewModel.SearchForm = new SearchFormViewModel("Search__Form") { Terms = viewModel.Terms };
            viewModel.SearchResults = new SearchResultsViewModel("Search__Results")
            {
                ContentItems = containedItems.OrderBy(x => searchResult.ContentItemIds.IndexOf(x.ContentItemId))
                .Take(pager.PageSize)
                .ToList(),
            };
            viewModel.Pager = (await New.PagerSlim(pager)).UrlParams(new Dictionary<string, string>()
            {
                { "Terms", viewModel.Terms }
            });
        });

        return View(shapeViewModel);
    }

    private async Task<ISearchService> GetSearchServiceAsync(IEnumerable<ISearchService> searchServices, int totalServices)
    {
        ISearchService searchService = null;

        if (totalServices > 1)
        {
            var site = await _siteService.GetSiteSettingsAsync();

            var searchSettings = site.As<SearchSettings>();

            if (!String.IsNullOrEmpty(searchSettings.SearchProviderAreaName))
            {
                var searchProvider = _serviceProvider.GetServices<SearchProvider>()
                    .FirstOrDefault(x => x.AreaName == searchSettings.SearchProviderAreaName);

                if (searchProvider != null)
                {
                    searchService = searchServices.FirstOrDefault(service => service.CanHandle(searchProvider));
                }
            }
        }

        return searchService ?? searchServices.First();
    }
}
