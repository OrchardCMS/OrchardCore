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
                    PagerParameters = pagerParameters,
                    IndexName = id,
                    ContentItems = Enumerable.Empty<ContentItem>()
                });
            }


            var queryParser = new MultiFieldQueryParser(_luceneSettings.GetVersion(), _luceneSettings.GetSearchFields(), _luceneSettings.GetAnalyzer());
            var query = queryParser.Parse(QueryParser.Escape(q));

            List<int> contentItemIds = new List<int>();

            _luceneIndexProvider.Search(indexName, searcher =>
            {
                var docs = searcher.Search(query, pager.PageSize);
                ScoreDoc[] hits = docs.ScoreDocs;

                foreach (var hit in hits)
                {
                    var d = searcher.Doc(hit.Doc, IdSet);
                    contentItemIds.Add(Convert.ToInt32(d.GetField("ContentItemId").StringValue));
                }
            });

            var contentItems = new List<ContentItem>();
            foreach(var contentItemId in contentItemIds)
            {
                var contentItem = await _contentManager.GetAsync(contentItemId);
                if (contentItem != null)
                {
                    contentItems.Add(contentItem);
                }
            }

            var model = new SearchIndexViewModel
            {
                Query = q,
                PagerParameters = pagerParameters,
                IndexName = id,
                ContentItems = contentItems
            };

            return View(model);
        }
    }
}
