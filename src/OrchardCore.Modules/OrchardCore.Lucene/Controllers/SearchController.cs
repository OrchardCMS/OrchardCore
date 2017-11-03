using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using OrchardCore.Lucene.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Lucene.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISiteService _siteService;
        private readonly LuceneIndexManager _luceneIndexProvider;
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly IContentManager _contentManager;

        private static HashSet<string> IdSet = new HashSet<string>(new string[] { "ContentItemId" });

        public SearchController(
            ISiteService siteService,
            LuceneIndexManager luceneIndexProvider,
            LuceneIndexingService luceneIndexingService,
            IContentManager contentManager,
            ILogger<SearchController> logger
            )
        {
            _siteService = siteService;
            _luceneIndexProvider = luceneIndexProvider;
            _luceneIndexingService = luceneIndexingService;
            _contentManager = contentManager;

            Logger = logger;
        }

        ILogger Logger { get; set; }

        public async Task<IActionResult> Index(string id, string q, PagerParameters pagerParameters)
        {
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

            if (luceneSettings == null)
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

            var contentItemIds = new List<string>();

            await _luceneIndexProvider.SearchAsync(indexName, searcher =>
            {
                // Fetch one more result than PageSize to generate "More" links
                var collector = TopScoreDocCollector.Create(pager.PageSize + 1, true);

                searcher.Search(query, collector);
                var hits = collector.GetTopDocs(pager.GetStartIndex(), pager.PageSize + 1);

                foreach (var hit in hits.ScoreDocs)
                {
                    var d = searcher.Doc(hit.Doc, IdSet);
                    contentItemIds.Add(d.GetField("ContentItemId").GetStringValue());
                }

                return Task.CompletedTask;
            });

            var contentItems = new List<ContentItem>();
            foreach(var contentItemId in contentItemIds.Take(pager.PageSize))
            {
                var contentItem = await _contentManager.GetAsync(contentItemId);
                if (contentItem != null)
                {
                    contentItems.Add(contentItem);
                }
            }

            var model = new SearchIndexViewModel
            {
                HasMoreResults = contentItemIds.Count > pager.PageSize,
                Query = q,
                Pager = pager,
                IndexName = id,
                ContentItems = contentItems
            };

            return View(model);
        }
    }
}
