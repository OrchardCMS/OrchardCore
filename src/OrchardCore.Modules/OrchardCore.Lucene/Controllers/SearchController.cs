using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers.Classic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement;
using OrchardCore.Lucene.Model;
using OrchardCore.Lucene.Services;
using OrchardCore.Navigation;
using OrchardCore.Search.Abstractions.ViewModels;
using OrchardCore.Settings;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Lucene.Controllers
{
    public class SearchController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;
        private readonly LuceneIndexManager _luceneIndexProvider;
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly ISearchQueryService _searchQueryService;
        private readonly ISession _session;
        private readonly dynamic New;

        public SearchController(
            IAuthorizationService authorizationService,
            ISiteService siteService,
            LuceneIndexManager luceneIndexProvider,
            LuceneIndexingService luceneIndexingService,
            ISearchQueryService searchQueryService,
            ISession session,
            IShapeFactory shapeFactory,
            ILogger<SearchController> logger
            )
        {
            _authorizationService = authorizationService;
            _siteService = siteService;
            _luceneIndexProvider = luceneIndexProvider;
            _luceneIndexingService = luceneIndexingService;
            _searchQueryService = searchQueryService;
            _session = session;
            New = shapeFactory;

            Logger = logger;
        }

        ILogger Logger { get; set; }

        [HttpGet]
        public async Task<IActionResult> Index(SearchIndexViewModel viewModel, PagerSlimParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneSearch))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();

            if (!_luceneIndexProvider.Exists(viewModel.IndexName))
            {
                Logger.LogInformation("Couldn't execute search. The search index doesn't exist.");
                return BadRequest("Search is not configured.");
            }

            var luceneSettings = await _luceneIndexingService.GetLuceneSettingsAsync();

            if (luceneSettings == null || luceneSettings?.DefaultSearchFields == null)
            {
                Logger.LogInformation("Couldn't execute search. No Lucene settings was defined.");
                return BadRequest("Search is not configured.");
            }

            if (string.IsNullOrWhiteSpace(viewModel.Terms))
            {
                return View(new SearchIndexViewModel
                {
                    IndexName = viewModel.IndexName,
                    ContentItems = Enumerable.Empty<ContentItem>()
                });
            }

            var pager = new PagerSlim(pagerParameters, siteSettings.PageSize);

            //We Query Lucene index
            var queryParser = new MultiFieldQueryParser(LuceneSettings.DefaultVersion, luceneSettings.DefaultSearchFields, new StandardAnalyzer(LuceneSettings.DefaultVersion));
            var query = queryParser.Parse(QueryParser.Escape(viewModel.Terms));

            // Fetch one more result than PageSize to generate "More" links
            var start = 0;
            var end = pager.PageSize + 1;

            if (pagerParameters.Before != null)
            {
                start = Int32.Parse(pagerParameters.Before) - pager.PageSize - 1;
                end = Int32.Parse(pagerParameters.Before);
            }
            else if (pagerParameters.After != null)
            {
                start = Int32.Parse(pagerParameters.After);
                end = Int32.Parse(pagerParameters.After) + pager.PageSize + 1;
            }

            var contentItemIds = await _searchQueryService.ExecuteQueryAsync(query, viewModel.IndexName, start, end);

            //We Query database to retrieve content items.
            var queryDb = _session.Query<ContentItem, ContentItemIndex>().Where(x => x.ContentItemId.IsIn(contentItemIds) && x.Published && x.Latest)
                .Take(pager.PageSize + 1);
            var containedItems = await queryDb.ListAsync();

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
                Pager = (await New.PagerSlim(pager)).UrlParams(new Dictionary<string, object>() { { "IndexName", viewModel.IndexName }, { "Terms", viewModel.Terms } }),
                IndexName = viewModel.IndexName,
                ContentItems = containedItems.Take(pager.PageSize)
            };

            return View(model);
        }
    }
}