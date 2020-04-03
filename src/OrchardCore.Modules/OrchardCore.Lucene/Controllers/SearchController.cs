using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers.Classic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Lucene.Services;
using OrchardCore.Lucene.ViewModels;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Lucene.Controllers
{
    public class SearchController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;
        private readonly LuceneIndexManager _luceneIndexProvider;
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly ISearchQueryService _searchQueryService;
        private readonly IContentManager _contentManager;

        public SearchController(
              IAuthorizationService authorizationService,
            ISiteService siteService,
            LuceneIndexManager luceneIndexProvider,
            LuceneIndexingService luceneIndexingService,
            ISearchQueryService searchQueryService,
            IContentManager contentManager,
            ILogger<SearchController> logger
            )
        {
            _authorizationService = authorizationService;
            _siteService = siteService;
            _luceneIndexProvider = luceneIndexProvider;
            _luceneIndexingService = luceneIndexingService;
            _searchQueryService = searchQueryService;
            _contentManager = contentManager;

            Logger = logger;
        }

        ILogger Logger { get; set; }

        public async Task<IActionResult> Index(string id, string q, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneSearch))
            {
                return NotFound();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            var indexName = "Search";

            if (!string.IsNullOrWhiteSpace(id))
            {
                indexName = id;
            }

            if (!_luceneIndexProvider.Exists(indexName))
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(q))
            {
                return View(new SearchIndexViewModel
                {
                    Pager = pager,
                    IndexName = id,
                    ContentItems = Enumerable.Empty<ContentItem>()
                });
            }

            var luceneSettings = await _luceneIndexingService.GetLuceneSettingsAsync();

            if (luceneSettings == null || luceneSettings?.DefaultSearchFields == null)
            {
                Logger.LogInformation("Couldn't execute search. No Lucene settings was defined.");

                return View(new SearchIndexViewModel
                {
                    HasMoreResults = false,
                    Query = q,
                    Pager = pager,
                    IndexName = id,
                    ContentItems = Enumerable.Empty<ContentItem>()
                });
            }

            var queryParser = new MultiFieldQueryParser(LuceneSettings.DefaultVersion, luceneSettings.DefaultSearchFields, new StandardAnalyzer(LuceneSettings.DefaultVersion));
            var query = queryParser.Parse(QueryParser.Escape(q));

            int start = pager.GetStartIndex(), size = pager.PageSize, end = size + 1;// Fetch one more result than PageSize to generate "More" links
            var contentItemIds = await _searchQueryService.ExecuteQueryAsync(query, indexName, start, end);

            var contentItems = new List<ContentItem>();
            foreach (var contentItemId in contentItemIds.Take(size))
            {
                var contentItem = await _contentManager.GetAsync(contentItemId);
                if (contentItem != null)
                {
                    contentItems.Add(contentItem);
                }
            }

            var model = new SearchIndexViewModel
            {
                HasMoreResults = contentItemIds.Count > size,
                Query = q,
                Pager = pager,
                IndexName = id,
                ContentItems = contentItems
            };

            return View(model);
        }
    }
}