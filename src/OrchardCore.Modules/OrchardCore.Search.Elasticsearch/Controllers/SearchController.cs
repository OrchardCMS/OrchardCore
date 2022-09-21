using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Nest;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement;
using OrchardCore.Entities;
using OrchardCore.Navigation;
using OrchardCore.Search.Abstractions.ViewModels;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Search.Elasticsearch
{
    public class SearchController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;
        private readonly ElasticIndexManager _elasticIndexManager;
        private readonly ElasticIndexingService _elasticIndexingService;
        private readonly ElasticIndexSettingsService _elasticIndexSettingsService;
        private readonly ElasticAnalyzerManager _elasticAnalyzerManager;
        private readonly IElasticSearchQueryService _searchQueryService;
        private readonly ISession _session;
        private readonly IStringLocalizer S;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;
        private readonly dynamic New;
        private readonly ILogger _logger;

        public SearchController(
            IAuthorizationService authorizationService,
            ISiteService siteService,
            ElasticIndexManager elasticIndexManager,
            ElasticIndexingService elasticIndexingService,
            ElasticIndexSettingsService elasticIndexSettingsService,
            ElasticAnalyzerManager elasticAnalyzerManager,
            IElasticSearchQueryService searchQueryService,
            ISession session,
            IStringLocalizer<SearchController> stringLocalizer,
            IEnumerable<IPermissionProvider> permissionProviders,
            IShapeFactory shapeFactory,
            ILogger<SearchController> logger
            )
        {
            _authorizationService = authorizationService;
            _siteService = siteService;
            _elasticIndexManager = elasticIndexManager;
            _elasticIndexingService = elasticIndexingService;
            _elasticIndexSettingsService = elasticIndexSettingsService;
            _elasticAnalyzerManager = elasticAnalyzerManager;
            _searchQueryService = searchQueryService;
            _session = session;
            S = stringLocalizer;
            _permissionProviders = permissionProviders;
            New = shapeFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Search(SearchIndexViewModel viewModel, PagerSlimParameters pagerParameters)
        {
            var permissionsProvider = _permissionProviders.FirstOrDefault(x => x.GetType().FullName == "OrchardCore.Search.Elasticsearch.Permissions");
            var permissions = await permissionsProvider.GetPermissionsAsync();

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var searchSettings = siteSettings.As<ElasticSettings>();

            if (permissions.FirstOrDefault(x => x.Name == "QueryElasticsearch" + searchSettings.SearchIndex + "Index") != null)
            {
                if (!await _authorizationService.AuthorizeAsync(User, permissions.FirstOrDefault(x => x.Name == "QueryElasticsearch" + searchSettings.SearchIndex + "Index")))
                {
                    return this.ChallengeOrForbid();
                }
            }
            else
            {
                _logger.LogInformation("Couldn't execute Elasticsearch search. The search index doesn't exist.");
                return BadRequest("Elasticsearch is not configured.");
            }

            if (searchSettings.SearchIndex != null && !await _elasticIndexManager.Exists(searchSettings.SearchIndex))
            {
                _logger.LogInformation("Couldn't execute Elasticsearch search. The Elasticsearch search index doesn't exist.");
                return BadRequest("Elasticsearch is not configured.");
            }

            var elasticSettings = await _elasticIndexingService.GetElasticSettingsAsync();

            if (elasticSettings == null || elasticSettings?.DefaultSearchFields == null)
            {
                _logger.LogInformation("Couldn't execute Elasticsearch search. No Elasticsearch settings was defined.");
                return BadRequest("Elasticsearch is not configured.");
            }

            var elasticIndexSettings = await _elasticIndexSettingsService.GetSettingsAsync(searchSettings.SearchIndex);

            if (elasticIndexSettings == null)
            {
                _logger.LogInformation("Couldn't execute search. No Elasticsearch index settings was defined for '{SearchIndex}' index.", searchSettings.SearchIndex);
                return BadRequest($"Search index ({searchSettings.SearchIndex}) is not configured.");
            }

            if (String.IsNullOrWhiteSpace(viewModel.Terms))
            {
                return View(new SearchIndexViewModel
                {
                    SearchForm = new SearchFormViewModel("Search__Form") { },
                });
            }

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

            var terms = viewModel.Terms;
            IList<string> contentItemIds;
            var analyzer = _elasticAnalyzerManager.CreateAnalyzer(await _elasticIndexSettingsService.GetIndexAnalyzerAsync(elasticIndexSettings.IndexName));

            try
            {
                QueryContainer query = null;

                if (searchSettings.AllowElasticQueryStringQueryInSearch)
                {
                    query = new QueryStringQuery
                    {
                        Fields = searchSettings.DefaultSearchFields,
                        Analyzer = analyzer.Type,
                        Query = terms
                    };
                }
                else
                {
                    query = new MultiMatchQuery
                    {
                        Fields = searchSettings.DefaultSearchFields,
                        Analyzer = analyzer.Type,
                        Query = terms
                    };
                }

                contentItemIds = (await _searchQueryService.ExecuteQueryAsync(searchSettings.SearchIndex, query, null, from, size))
                    .ToList();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Terms", S["Incorrect query syntax."]);
                _logger.LogError(e, "Incorrect Elasticsearch search query syntax provided in search:");

                // Return a SearchIndexViewModel without SearchResults or Pager shapes since there is an error.
                return View(new SearchIndexViewModel
                {
                    Terms = viewModel.Terms,
                    SearchForm = new SearchFormViewModel("Search__Form") { Terms = viewModel.Terms },
                });
            }

            // We Query database to retrieve content items.
            IQuery<ContentItem> queryDb;

            if (elasticIndexSettings.IndexLatest)
            {
                queryDb = _session.Query<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentItemId.IsIn(contentItemIds) && x.Latest == true)
                    .Take(pager.PageSize + 1);
            }
            else
            {
                queryDb = _session.Query<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentItemId.IsIn(contentItemIds) && x.Published == true)
                    .Take(pager.PageSize + 1);
            }

            // Sort the content items by their rank in the search results returned by Elasticsearch.
            var containedItems = (await queryDb.ListAsync()).OrderBy(x => contentItemIds.IndexOf(x.ContentItemId));

            // We set the PagerSlim before and after links
            if (pagerParameters.After != null || pagerParameters.Before != null)
            {
                if (from + 1 > 1)
                {
                    pager.Before = (from + 1).ToString();
                }
                else
                {
                    pager.Before = null;
                }
            }

            if (containedItems.Count() == pager.PageSize + 1)
            {
                pager.After = (size - 1).ToString();
            }
            else
            {
                pager.After = null;
            }

            var model = new SearchIndexViewModel
            {
                Terms = viewModel.Terms,
                SearchForm = new SearchFormViewModel("Search__Form") { Terms = viewModel.Terms },
                SearchResults = new SearchResultsViewModel("Search__Results") { ContentItems = containedItems.Take(pager.PageSize) },
                Pager = (await New.PagerSlim(pager)).UrlParams(new Dictionary<string, string>() { { "Terms", viewModel.Terms } })
            };

            return View(model);
        }
    }
}
