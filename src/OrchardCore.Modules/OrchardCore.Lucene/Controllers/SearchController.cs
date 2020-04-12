using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.QueryParsers.Classic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement;
using OrchardCore.Entities;
using OrchardCore.Lucene.Model;
using OrchardCore.Lucene.Services;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Search.Abstractions.ViewModels;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Lucene.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchPermissionService _searchPermissionService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;        
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;
        private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
        private readonly ISearchQueryService _searchQueryService;
        private readonly ISession _session;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;
        private readonly dynamic New;
        private readonly ILogger _logger;

        public SearchController(
            ISearchPermissionService searchPermissionService,
            IAuthorizationService authorizationService,
            ISiteService siteService,            
            LuceneIndexingService luceneIndexingService,
            LuceneIndexSettingsService luceneIndexSettingsService,
            LuceneAnalyzerManager luceneAnalyzerManager,
            ISearchQueryService searchQueryService,
            ISession session,
            IEnumerable<IPermissionProvider> permissionProviders,
            IShapeFactory shapeFactory,
            ILogger<SearchController> logger
            )
        {
            _searchPermissionService = searchPermissionService;
            _authorizationService = authorizationService;
            _siteService = siteService;            
            _luceneIndexingService = luceneIndexingService;
            _luceneIndexSettingsService = luceneIndexSettingsService;
            _luceneAnalyzerManager = luceneAnalyzerManager;
            _searchQueryService = searchQueryService;
            _session = session;
            _permissionProviders = permissionProviders;
            New = shapeFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Search(SearchIndexViewModel viewModel, PagerSlimParameters pagerParameters)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var searchSettings = siteSettings.As<LuceneSettings>();       

            if (searchSettings?.DefaultSearchFields == null)
            {
                _logger.LogInformation("Couldn't execute search. The search index doesn't exist.");
                
                return BadRequest("Search is not configured.");
            }

            string indexName = searchSettings.SearchIndex;
            var result = await _searchPermissionService.CheckPermission(indexName, User);
            if(result.Forbidden) return this.ChallengeOrForbid();
            if (result.Failed)
            {
                foreach (var error in result.Errors)
                {
                    return BadRequest(error.ToString());
                }
            }

            var luceneIndexSettings = await _luceneIndexSettingsService.GetSettingsAsync(indexName);

            if (luceneIndexSettings == null)
            {
                _logger.LogInformation($"Couldn't execute search. No Lucene index settings was defined for ({searchSettings.SearchIndex}) index.");
                return BadRequest($"Search index ({indexName}) is not configured.");
            }

            if (string.IsNullOrWhiteSpace(viewModel.Terms))
            {
                return View(new SearchIndexViewModel
                {
                    SearchForm = new SearchFormViewModel("Search__Form") { },
                });
            }

            var pager = new PagerSlim(pagerParameters, siteSettings.PageSize);

            // We Query Lucene index
            var analyzer = _luceneAnalyzerManager.CreateAnalyzer(await _luceneIndexSettingsService.GetIndexAnalyzerAsync(luceneIndexSettings.IndexName));
            var queryParser = new MultiFieldQueryParser(LuceneSettings.DefaultVersion, searchSettings.DefaultSearchFields, analyzer);
            var query = queryParser.Parse(QueryParser.Escape(viewModel.Terms));

            // Fetch one more result than PageSize to generate "More" links
            var start = 0;
            var end = pager.PageSize + 1;

            if (pagerParameters.Before != null)
            {
                start = Convert.ToInt32(pagerParameters.Before) - pager.PageSize - 1;
                end = Convert.ToInt32(pagerParameters.Before);
            }
            else if (pagerParameters.After != null)
            {
                start = Convert.ToInt32(pagerParameters.After);
                end = Convert.ToInt32(pagerParameters.After) + pager.PageSize + 1;
            }

            var contentItemIds = await _searchQueryService.ExecuteQueryAsync(query, indexName, start, end);

            // We Query database to retrieve content items.
            IQuery<ContentItem> queryDb;

            if (luceneIndexSettings.IndexLatest)
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

            var containedItems = await queryDb.ListAsync();

            // We set the PagerSlim before and after links
            if (pagerParameters.After != null || pagerParameters.Before != null)
            {
                if (start + 1 > 1)
                {
                    pager.Before = (start + 1).ToString();
                }
                else
                {
                    pager.Before = null;
                }
            }

            if (containedItems.Count() == pager.PageSize + 1)
            {
                pager.After = (end - 1).ToString();
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
