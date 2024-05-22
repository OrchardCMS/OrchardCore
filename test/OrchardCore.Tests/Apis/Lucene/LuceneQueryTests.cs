using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Html.Models;
using OrchardCore.Queries;
using OrchardCore.Search.Lucene;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Utilities;

namespace OrchardCore.Tests.Apis.Lucene
{
    public class LuceneQueryTests
    {
        [Fact]
        public async Task BoostingTitleShouldHaveTitlesContainingOrchardAppearFirst()
        {
            using var context = new LuceneContext();
            await context.InitializeAsync();

            // Act
            var index = "ArticleIndex";
            // { "from": 0, "size": 2, "query": { "simple_query_string": { "analyze_wildcard": true, "fields": ["Content.ContentItem.DisplayText_Normalized^2", "Content.BodyAspect.Body"], "query": "orchard*" } } }
            var dynamicQuery = new
            {
                from = 0,
                size = 2,
                query = new
                {
                    simple_query_string = new
                    {
                        analyze_wildcard = true,
                        fields = new string[] { "Content.ContentItem.DisplayText.Normalized^2", "Content.BodyAspect.Body" },
                        query = "orchard*",
                    },
                },
            };

            var query = JConvert.SerializeObject(dynamicQuery);

            var content = await context.Client.GetAsync($"api/lucene/content?indexName={index}&query={query}");
            var queryResults = await content.Content.ReadAsAsync<Search.Lucene.LuceneQueryResults>();

            // Test
            Assert.Equal(2, queryResults.Items.Count());
            var contentItems = queryResults.Items.Select(x => JObject.FromObject(x).Deserialize<ContentItem>());

            Assert.True(contentItems.All(x => x.DisplayText.Contains("Orchard", StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public async Task BoostingBodyShouldHaveTitlesNotContainingOrchardAppearFirst()
        {
            using var context = new LuceneContext();
            await context.InitializeAsync();

            // Act
            var index = "ArticleIndex";
            // { "from": 0, "size": 2, "query": { "simple_query_string": { "analyze_wildcard": true, "fields": ["Content.ContentItem.DisplayText_Normalized", "Content.BodyAspect.Body^2"], "query": "orchard*" } } }
            var dynamicQuery = new
            {
                from = 0,
                size = 2,
                query = new
                {
                    simple_query_string = new
                    {
                        analyze_wildcard = true,
                        fields = new string[] { "Content.ContentItem.DisplayText_Normalized", "Content.BodyAspect.Body^2" },
                        query = "orchard*",
                    },
                },
            };

            var query = JConvert.SerializeObject(dynamicQuery);
            var content = await context.Client.GetAsync($"api/lucene/content?indexName={index}&query={query}");
            var queryResults = await content.Content.ReadAsAsync<Search.Lucene.LuceneQueryResults>();

            // Test
            Assert.Equal(2, queryResults.Items.Count());
            var contentItems = queryResults.Items.Select(x => JObject.FromObject(x).Deserialize<ContentItem>());

            Assert.False(contentItems.All(x => x.DisplayText.Contains("Orchard", StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public async Task SimpleQueryWildcardHasResults()
        {
            using var context = new LuceneContext();
            await context.InitializeAsync();

            // Act
            // Should find articles with "Orchard" in the title.

            var index = "ArticleIndex";

            // { "from": 0, "size": 1, "query": { "simple_query_string": { "analyze_wildcard": true, "fields": ["Content.ContentItem.DisplayText.Normalized"], "query": "orch*" } } }
            object dynamicQuery = new
            {
                from = 0,
                size = 1,
                query = new
                {
                    simple_query_string = new
                    {
                        analyze_wildcard = true,
                        fields = new string[] { "Content.ContentItem.DisplayText.Normalized" },
                        query = "orch*",
                    },
                },
            };

            var query = JConvert.SerializeObject(dynamicQuery);

            var content = await context.Client.GetAsync($"api/lucene/content?indexName={index}&query={query}");
            var queryResults = await content.Content.ReadAsAsync<Search.Lucene.LuceneQueryResults>();

            // Test
            Assert.NotEmpty(queryResults.Items);
            var contentItems = queryResults.Items.Select(x => JObject.FromObject(x).Deserialize<ContentItem>());

            Assert.Contains("Orchard", contentItems.First().DisplayText, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task TwoWildcardQueriesWithBoostHasResults()
        {
            using (var context = new LuceneContext())
            {
                await context.InitializeAsync();

                // Act
                // Should find articles with "Orchard" in the title
                var index = "ArticleIndex";

                // { "from": 0, "size": 10, "query":{ "bool": { "should": [ { "wildcard": {  "Content.ContentItem.DisplayText.Normalized": { "value": "orch*", "boost": 2 } } },{ "wildcard": { "Content.BodyAspect.Body": { "value": "orchar*", "boost": 5 } } } ] } } }
                var query =
                    "{ \"from\": 0, \"size\": 10, \"query\":" +
                        "{ \"bool\": { \"should\": [ " +
                            "{ \"wildcard\": {  \"Content.ContentItem.DisplayText.Normalized\": { \"value\": \"orch*\", \"boost\": 2 } } }," +
                            "{ \"wildcard\": { \"Content.BodyAspect.Body\": { \"value\": \"orchar*\", \"boost\": 5 } } }" +
                        "] } } }";

                var content = await context.Client.GetAsync($"api/lucene/content?indexName={index}&query={query}");
                var queryResults = await content.Content.ReadAsAsync<Search.Lucene.LuceneQueryResults>();
                var contentItems = queryResults.Items.Select(x => JObject.FromObject(x).Deserialize<ContentItem>());

                // Test
                Assert.NotEmpty(contentItems);
                Assert.True(contentItems.Count() >= 4);

                Assert.Contains("Orchard", contentItems.ElementAt(0).As<HtmlBodyPart>().Html, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("Orchard", contentItems.ElementAt(1).As<HtmlBodyPart>().Html, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("Orchard", contentItems.ElementAt(2).DisplayText, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("Orchard", contentItems.ElementAt(3).DisplayText, StringComparison.OrdinalIgnoreCase);
            };
        }




        [Fact]
        public async Task ShouldUpdateIndexContentAfterContentMangerAction()
        {
            using var context = new BlogContext();
            await context.InitializeAsync();

            await context.UsingTenantScopeAsync(async scope =>
            {
                var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

                var existContent = await contentManager.GetAsync(context.BlogContentItemId);

                var firstContent = await contentManager.CloneAsync(existContent);
                firstContent.Alter<AutoroutePart>(part =>
                {
                    part.Path = "test1";
                });
                var luceneQuery = new LuceneQuery
                {
                    Index = "Search",
                    ReturnContentItems = false,
                    Template = @"
                            {
                                ""query"": {
                                    ""term"": { ""Content.ContentItem.ContentType"": ""BlogPost"" }
                                },
                                ""filter"": [
                                    {
                                        ""term"": { ""Content.ContentItem.ContentItemId"":""{{parameters.contentItemId}}"" }
                                    }
                                ]
                            }",
                };
                var queryParameters = new Dictionary<string, object>
                {
                    ["contentItemId"] = firstContent.ContentItemId
                };

                #region Create a content item

                
                //await contentManager.CreateAsync(firstContent);
                //var contentFromDb = await contentManager.GetAsync(firstContent.ContentItemId);
                //Assert.NotNull(contentFromDb); faild

                await contentManager.UpdateValidateAndCreateAsync(firstContent, VersionOptions.Draft);
                await contentManager.PublishAsync(firstContent);
                var contentFromDb = await contentManager.GetAsync(firstContent.ContentItemId);
                Assert.NotNull(contentFromDb);

                var initialContentItemVersionId = firstContent.ContentItemVersionId;

                var luceneQuerySource = scope.ServiceProvider.GetRequiredService<LuceneQuerySource>();
                var result = await ValidateLuceneIndexing(luceneQuerySource, luceneQuery, queryParameters);

                Assert.Single(result.Items);
                Assert.Equal(firstContent.ContentItemId, JObject.FromObject(result).SelectNode("$.items[0].ContentItemId").ToString());
                Assert.NotEqual(initialContentItemVersionId, JObject.FromObject(result).SelectNode("$.items[0].ContentItemVersionId").ToString());

                #endregion

                #region Publish a content item.
                await contentManager.PublishAsync(firstContent);
                result = await ValidateLuceneIndexing(luceneQuerySource, luceneQuery, queryParameters);

                Assert.Single(result.Items);
                Assert.Equal(firstContent.ContentItemId, JObject.FromObject(result).SelectNode("$.items[0].ContentItemId").ToString());
                Assert.Equal(initialContentItemVersionId, JObject.FromObject(result).SelectNode("$.items[0].ContentItemVersionId").ToString());
                #endregion

                #region Save a draft of a content item
                await contentManager.SaveDraftAsync(firstContent);
                result = await ValidateLuceneIndexing(luceneQuerySource, luceneQuery, queryParameters);

                Assert.Single(result.Items);
                Assert.Equal(firstContent.ContentItemId, JObject.FromObject(result).SelectNode("$.items[0].ContentItemId").ToString());
                Assert.Equal(initialContentItemVersionId, JObject.FromObject(result).SelectNode("$.items[0].ContentItemVersionId").ToString());
                #endregion

                #region Remove a content items.
                await contentManager.RemoveAsync(firstContent);
                result = await ValidateLuceneIndexing(luceneQuerySource, luceneQuery, queryParameters);

                Assert.Empty(result.Items);
                #endregion

            });
        }

        private static async Task<IQueryResults> ValidateLuceneIndexing(LuceneQuerySource luceneQuerySource, LuceneQuery luceneQuery, Dictionary<string, object> queryParameters)
        {
            var loopCount = 4;
            IQueryResults result = null;
            await TimeoutTaskRunner.RunAsync(new TimeoutTaskOption
            {
                Timeout = TimeSpan.FromSeconds(5),
                Interval = 500,
                TimeoutMessage = "The Lucene index wasn't updated after the operation within 5s and thus the test timed out.",
                CanNextLoop = async () =>
                {
                    loopCount++;
                    // Ensure adequate waiting time.
                    if (loopCount < 4)
                    {
                        return true;
                    }

                    result = await luceneQuerySource.ExecuteQueryAsync(luceneQuery, queryParameters);

                    if (result is not null)
                    {
                        return false;
                    }
                    return true; // continue;
                }
            });
            return result;
        }
    }
}
