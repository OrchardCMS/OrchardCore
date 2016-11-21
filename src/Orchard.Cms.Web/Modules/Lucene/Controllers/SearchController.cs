using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Orchard.ContentManagement;
using Orchard.Navigation;
using Orchard.Settings;

namespace Lucene.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISiteService _siteService;
        private readonly LuceneIndexProvider _luceneIndexProvider;
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly LuceneSettings _luceneSettings;
        private readonly IContentManager _contentManager;

        private static HashSet<string> IdSet = new HashSet<string>(new string[] { "ContentItemId" });

        public SearchController(
            ISiteService siteService,
            LuceneIndexProvider luceneIndexProvider,
            LuceneIndexingService luceneIndexingService,
            LuceneSettings luceneSettings,
            IContentManager contentManager
            )
        {
            _siteService = siteService;
            _luceneIndexProvider = luceneIndexProvider;
            _luceneIndexingService = luceneIndexingService;
            _luceneSettings = luceneSettings;
            _contentManager = contentManager;
        }

        public async Task<IActionResult> Index(string id, string q, PagerParameters pagerParameters)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            Pager pager = new Pager(pagerParameters, siteSettings.PageSize);

            var indexName = "Search";

            if (!String.IsNullOrWhiteSpace(id))
            {
                indexName = id;
            }

            if (!_luceneIndexProvider.Exists(indexName))
            {
                return NotFound();
            }

            if (String.IsNullOrWhiteSpace(q))
            {
                return View(new SearchIndexViewModel
                {
                    Pager = pager,
                    IndexName = id,
                    ContentItems = Enumerable.Empty<ContentItem>()
                });
            }

            var queryParser = new MultiFieldQueryParser(_luceneSettings.GetVersion(), _luceneSettings.GetSearchFields(), _luceneSettings.GetAnalyzer());
            var query = queryParser.Parse(QueryParser.Escape(q));

            List<int> contentItemIds = new List<int>();

            _luceneIndexProvider.Search(indexName, searcher =>
            {
                // Fetch one more result than PageSize to generate "More" links
                TopScoreDocCollector collector = TopScoreDocCollector.Create(pager.PageSize + 1, true);

                searcher.Search(query, collector);
                TopDocs hits = collector.TopDocs(pager.GetStartIndex(), pager.PageSize + 1);

                foreach (var hit in hits.ScoreDocs)
                {
                    var d = searcher.Doc(hit.Doc, IdSet);
                    contentItemIds.Add(Convert.ToInt32(d.GetField("ContentItemId").StringValue));
                }
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
