using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Html.Models;
using OrchardCore.Lucene;
using OrchardCore.Lucene.Model;
using OrchardCore.Lucene.Services;
using OrchardCore.Title.Models;
using Xunit;

namespace OrchardCore.Tests.Lucene
{
    public class LuceneQueryTests
    {
        private static HashSet<string> IdSet = new HashSet<string>(new string[] { "ContentItemId" });

        [Fact]
        public async Task BoostedFieldsShouldAffectScore()
        {
            await SiteContext.ExecuteTest<LuceneContext>(async scope =>
            {
                // Act
                // Articles with "Orchard" in Title should appear first
                string titleBoost = "^2";
                string bodyBoost = string.Empty;
                string baseQuery = "{ \"from\": 0, \"size\": 2, \"query\": { \"simple_query_string\": { \"analyze_wildcard\": true, \"fields\": [ \"TitlePart{0}\", \"HtmlBodyPart{1}\" ], \"query\": \"orchard*\" } }}";

                string titleBoostQuery = baseQuery.Replace("{0}", titleBoost).Replace("{1}", bodyBoost);
                var contentItems = await SearchItems(scope, titleBoostQuery, "ArticleIndex");

                Assert.True(contentItems.All(x => x.As<TitlePart>().Title.Contains("Orchard", StringComparison.OrdinalIgnoreCase)));

                // Articles with "Orchard" in Title should NOT appear first
                titleBoost = string.Empty;
                bodyBoost = "^2";

                string bodyBoostQuery = baseQuery.Replace("{0}", titleBoost).Replace("{1}", bodyBoost);
                contentItems = await SearchItems(scope, bodyBoostQuery, "ArticleIndex");

                Assert.False(contentItems.All(x => x.As<TitlePart>().Title.Contains("Orchard", StringComparison.OrdinalIgnoreCase)));
            });
        }

        [Fact]
        public async Task SimpleQueryWildcardHasResults()
        {
            await SiteContext.ExecuteTest<LuceneContext>(async scope =>
            {
                // Act
                // Should find articles with "Orchard" in the title
                string query = "{ \"from\": 0, \"size\": 1, \"query\": { \"simple_query_string\": { \"analyze_wildcard\": true, \"fields\": [ \"TitlePart\" ], \"query\": \"orch*\" } }}";

                var contentItems = await SearchItems(scope, query, "ArticleIndex");

                Assert.NotEmpty(contentItems);
                Assert.Contains("Orchard", contentItems.First().As<TitlePart>().Title, StringComparison.OrdinalIgnoreCase);
            });
        }

        [Fact]
        public async Task WildcardQueryWithBoostHasResults()
        {
            await SiteContext.ExecuteTest<LuceneContext>(async scope =>
            {
                // Act
                // Should find articles with "Orchard" in the title
                string query = "{ \"from\": 0, \"size\": 10, \"query\": { \"bool\": { \"should\": [ { \"wildcard\": {  \"TitlePart\": { \"value\": \"orch*\", \"boost\": 2 } } }, { \"wildcard\": { \"HtmlBodyPart\": { \"value\": \"orchar*\", \"boost\": 5 } } } ] } } }";

                var contentItems = await SearchItems(scope, query, "ArticleIndex");

                Assert.NotEmpty(contentItems);
                Assert.True(contentItems.Count() >= 4);

                Assert.Contains("Orchard", contentItems.ElementAt(0).As<HtmlBodyPart>().Html, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("Orchard", contentItems.ElementAt(1).As<HtmlBodyPart>().Html, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("Orchard", contentItems.ElementAt(contentItems.Count() - 2).As<TitlePart>().Title, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("Orchard", contentItems.ElementAt(contentItems.Count() - 1).As<TitlePart>().Title, StringComparison.OrdinalIgnoreCase);
            });
        }

        private async Task<IEnumerable<ContentItem>> SearchItems(ShellScope scope, string query, string indexName)
        {
            var queryService = scope.ServiceProvider.GetRequiredService<ILuceneQueryService>();
            var luceneIndexManager = scope.ServiceProvider.GetRequiredService<LuceneIndexManager>();
            var luceneIndexSettingsService = scope.ServiceProvider.GetRequiredService<LuceneIndexSettingsService>();
            var luceneAnalyzerManager = scope.ServiceProvider.GetRequiredService<LuceneAnalyzerManager>();
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

            var contentItemIds = new List<string>();

            await luceneIndexManager.SearchAsync(indexName, async searcher =>
            {
                var analyzer = luceneAnalyzerManager.CreateAnalyzer(await luceneIndexSettingsService.GetIndexAnalyzerAsync(indexName));
                var context = new LuceneQueryContext(searcher, LuceneSettings.DefaultVersion, analyzer);
                var parameterizedQuery = JObject.Parse(query);
                var luceneTopDocs = await queryService.SearchAsync(context, parameterizedQuery);

                foreach (var hit in luceneTopDocs.TopDocs.ScoreDocs)
                {
                    var d = searcher.Doc(hit.Doc, IdSet);
                    contentItemIds.Add(d.GetField("ContentItemId").GetStringValue());
                }
            });

            return await contentManager.GetAsync(contentItemIds);
        }
    }
}
