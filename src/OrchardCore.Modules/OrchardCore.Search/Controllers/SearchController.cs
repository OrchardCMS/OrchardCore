using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Navigation;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Abstractions.ViewModels;
using OrchardCore.Search.Model;
using OrchardCore.Settings;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Search;

public class SearchController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ISiteService _siteService;
    private readonly ISession _session;
    private readonly IServiceProvider _serviceProvider;
    private readonly IShapeFactory _shapeFactory;
    private readonly dynamic New;
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer H;

    public SearchController(
        IAuthorizationService authorizationService,
        ISiteService siteService,
        ISession session,
        IServiceProvider serviceProvider,
        INotifier notifier,
        IShapeFactory shapeFactory,
        IHtmlLocalizer<SearchController> htmlLocalizer
        )
    {
        _authorizationService = authorizationService;
        _siteService = siteService;
        _session = session;
        _serviceProvider = serviceProvider;
        _shapeFactory = shapeFactory;
        _notifier = notifier;

        New = shapeFactory;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Search(SearchIndexViewModel viewModel, PagerSlimParameters pagerParameters)
    {
        var searchServices = _serviceProvider.GetServices<ISearchService>();

        if (!searchServices.Any())
        {
            await _notifier.WarningAsync(H["No search provider feature is enabled."]);

            return View();
        }

        var siteSettings = await _siteService.GetSiteSettingsAsync();

        var searchSettings = siteSettings.As<SearchSettings>();
        ISearchService searchService = null;

        if (!String.IsNullOrEmpty(searchSettings.ProviderName))
        {
            searchService = searchServices.FirstOrDefault(service => service.Name == searchSettings.ProviderName);
        }

        searchService ??= searchServices.First();

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.QuerySearchIndex, new SearchPermissionParameters(searchService.Name, viewModel.Index)))
        {
            return this.ChallengeOrForbid();
        }

        if (String.IsNullOrWhiteSpace(viewModel.Terms))
        {
            return View(await GetEmptyShape(viewModel, searchSettings));
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

        var searchResult = await searchService.SearchAsync(viewModel.Index, viewModel.Terms, from, size);

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

        var shape = await _shapeFactory.CreateAsync<SearchIndexViewModel>("Search__List", async model =>
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
        return await _shapeFactory.CreateAsync<SearchIndexViewModel>("Search__List", model =>
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
}
