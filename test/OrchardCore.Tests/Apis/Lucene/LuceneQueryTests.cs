using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.Html.Models;
using OrchardCore.Search.Lucene;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.Lucene;

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
        var queryResults = await content.Content.ReadAsAsync<LuceneQueryResults>();

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
        var queryResults = await content.Content.ReadAsAsync<LuceneQueryResults>();

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
        var queryResults = await content.Content.ReadAsAsync<LuceneQueryResults>();

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

            var query = """
                {
                    "from": 0,
                    "size": 10,
                    "query": {
                        "bool": {
                            "should": [
                                {
                                    "wildcard": {
                                        "Content.ContentItem.DisplayText.Normalized": {
                                            "value": "orch*",
                                            "boost": 2
                                        }
                                    }
                                },
                                {
                                    "wildcard": {
                                        "Content.BodyAspect.Body": {
                                            "value": "orchar*",
                                            "boost": 5
                                        }
                                    }
                                }
                            ]
                        }
                    }
                }
            """;
            var content = await context.Client.GetAsync($"api/lucene/content?indexName={index}&query={query}");
            var queryResults = await content.Content.ReadAsAsync<LuceneQueryResults>();
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
    public async Task LuceneQueryTemplateWithSpecialCharactersShouldNotThrowError()
    {
        using var context = new LuceneContext();
        await context.InitializeAsync();

        // Act
        var index = "ArticleIndex";
        var queryTemplate = "\r\r\n{% assign testVariable = \"48yvsghn194eft8axztaves25h\" %}\n\n{\n  \"query\": {\n    \"bool\": {\n      \"must\": [\n        { \"term\" : { \"Content.ContentItem.ContentType\" : \"Article\" } },\n        { \"term\": { \"Content.ContentItem.Published\" : \"true\" } },\n      ]\n    }\n  }\n}";

        var content = await context.Client.GetAsync($"api/lucene/content?indexName={index}&query={queryTemplate}");
        var queryResults = await content.Content.ReadAsAsync<LuceneQueryResults>();

        // Assert
        Assert.NotNull(queryResults);
        Assert.NotEmpty(queryResults.Items);

        var contentItems = queryResults.Items.Select(x => JObject.FromObject(x).Deserialize<ContentItem>());

        Assert.Contains("Orchard", contentItems.First().DisplayText, StringComparison.OrdinalIgnoreCase);
    }
}
