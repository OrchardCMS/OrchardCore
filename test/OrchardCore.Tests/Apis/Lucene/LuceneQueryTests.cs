using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Html.Models;
using OrchardCore.Tests.Apis.Context;
using Xunit;
using OrchardCore.Queries;
using Newtonsoft.Json;

namespace OrchardCore.Tests.Apis.Lucene
{
    public class LuceneQueryTests
    {
        [Fact]
        public async Task BoostedFieldsShouldAffectScore()
        {
            using (var context = new LuceneContext())
            {
                await context.InitializeAsync();

                string index = "ArticleIndex";
                dynamic dynamicQuery = new {
                    from = 0,
                    size = 2,
                    query = new {
                        simple_query_string = new
                        {
                            analyze_wildcard = true,
                            fields = new string[] { "Content.ContentItem.DisplayText.Normalized{0}", "HtmlBodyPart{1}" },
                            query = "orchard*"
                        }
                    }
                };

                string baseQuery = JsonConvert.SerializeObject(dynamicQuery);

                // Articles with "Orchard" in Title should appear first
                string titleBoost = "^2";
                string bodyBoost = string.Empty;
                string titleBoostQuery = baseQuery.Replace("{0}", titleBoost).Replace("{1}", bodyBoost);

                var content = await context.Client.GetAsync($"api/lucene/content?indexName={index}&query={titleBoostQuery}");
                var queryResults = await content.Content.ReadAsAsync<LuceneQueryResults>();

                Assert.Equal(2, queryResults.Items.Count());
                var contentItems = queryResults.Items.Select(x => ((JObject)x).ToObject<ContentItem>());
                Assert.True(contentItems.All(x => x.DisplayText.Contains("Orchard", StringComparison.OrdinalIgnoreCase)));

                // Articles with "Orchard" in Title should NOT appear first
                titleBoost = string.Empty;
                bodyBoost = "^2";

                string bodyBoostQuery = baseQuery.Replace("{0}", titleBoost).Replace("{1}", bodyBoost);
                content = await context.Client.GetAsync($"api/lucene/content?indexName={index}&query={bodyBoostQuery}");
                queryResults = await content.Content.ReadAsAsync<LuceneQueryResults>();

                contentItems = queryResults.Items.Select(x => ((JObject)x).ToObject<ContentItem>());

                Assert.False(contentItems.All(x => x.DisplayText.Contains("Orchard", StringComparison.OrdinalIgnoreCase)));
            }
        }

        [Fact]
        public async Task SimpleQueryWildcardHasResults()
        {
            using (var context = new LuceneContext())
            {
                await context.InitializeAsync();
                // Act
                // Should find articles with "Orchard" in the title
                string index = "ArticleIndex";
                dynamic dynamicQuery = new
                {
                    from = 0,
                    size = 1,
                    query = new
                    {
                        simple_query_string = new
                        {
                            analyze_wildcard = true,
                            fields = new string[] { "Content.ContentItem.DisplayText.Normalized" },
                            query = "orch*"
                        }
                    }
                };

                string query = JsonConvert.SerializeObject(dynamicQuery);

                var content = await context.Client.GetAsync($"api/lucene/content?indexName={index}&query={query}");
                var queryResults = await content.Content.ReadAsAsync<LuceneQueryResults>();

                Assert.NotEmpty(queryResults.Items);
                var contentItems = queryResults.Items.Select(x => ((JObject)x).ToObject<ContentItem>());

                Assert.Contains("Orchard", contentItems.First().DisplayText, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public async Task TwoWildcardQueriesWithBoostHasResults()
        {
            using (var context = new LuceneContext())
            {
                await context.InitializeAsync();

                // Act
                // Should find articles with "Orchard" in the title
                string index = "ArticleIndex";
                string query =
                    "{ \"from\": 0, \"size\": 10, \"query\":" +
                        "{ \"bool\": { \"should\": [ " +
                            "{ \"wildcard\": {  \"Content.ContentItem.DisplayText.Normalized\": { \"value\": \"orch*\", \"boost\": 2 } } }," +
                            "{ \"wildcard\": { \"HtmlBodyPart\": { \"value\": \"orchar*\", \"boost\": 5 } } }" +
                        "] } } }";

                var content = await context.Client.GetAsync($"api/lucene/content?indexName={index}&query={query}");
                var queryResults = await content.Content.ReadAsAsync<LuceneQueryResults>();
                var contentItems = queryResults.Items.Select(x => ((JObject)x).ToObject<ContentItem>());

                Assert.NotEmpty(contentItems);
                Assert.True(contentItems.Count() >= 4);

                Assert.Contains("Orchard", contentItems.ElementAt(0).As<HtmlBodyPart>().Html, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("Orchard", contentItems.ElementAt(1).As<HtmlBodyPart>().Html, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("Orchard", contentItems.ElementAt(contentItems.Count() - 2).DisplayText, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("Orchard", contentItems.ElementAt(contentItems.Count() - 1).DisplayText, StringComparison.OrdinalIgnoreCase);
            };
        }
    }
}
