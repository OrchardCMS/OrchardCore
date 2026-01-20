using System.Text.Json.Nodes;
using Lucene.Net.Analysis;
using Lucene.Net.Search;
using OrchardCore.Search.Lucene;
using OrchardCore.Search.Lucene.QueryProviders;

namespace OrchardCore.Tests.Modules.OrchardCore.Search.Lucene;

public partial class BooleanQueryProviderTests
{
    [Fact]
    public void BoolQueryFilterAsArrayDoesNotThrow()
    {
        var arrayFilter = JsonNode.Parse("""
        {
            "filter": [
              { "term": { "Content.ContentItem.ContentType": "BlogPost" } }
            ]
        }
        """)!;
        
        RunBoolQueryFilterTest(arrayFilter.AsObject());
    }
    
    [Fact]
    public void BoolQueryFilterAsObjectDoesNotThrow()
    {
        var objectFilter = JsonNode.Parse("""
        {
            "filter": { "term": { "Content.ContentItem.ContentType": "BlogPost" } }
        }
        """)!;

        RunBoolQueryFilterTest(objectFilter.AsObject());
    }

    private static void RunBoolQueryFilterTest(JsonObject filter)
    {
        // Arrange
        var luceneQueryService = new Mock<ILuceneQueryService>();
        var booleanQueryProvider = new BooleanQueryProvider([]);
        var luceneQueryContext = new Mock<LuceneQueryContext>(It.IsAny<IndexSearcher>(), LuceneConstants.DefaultVersion, It.IsAny<Analyzer>());

        // Act
        var filterQuery = booleanQueryProvider.CreateQuery(luceneQueryService.Object, luceneQueryContext.Object, "bool", filter);

        // Assert, null means there were no exceptions, we don't care about the actual result here.
        Assert.Null(filterQuery);
    }
}
