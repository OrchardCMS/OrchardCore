using System.Globalization;
using System.Text.Json.Nodes;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using OrchardCore.Localization;
using OrchardCore.Lucene;
using OrchardCore.Lucene.QueryProviders;

namespace OrchardCore.Tests.Modules.OrchardCore.Lucene;

public class LuceneQueryServiceSortTests
{
    [Theory]
    [InlineData("en-US")]
    [InlineData("tr-TR")]
    public async Task SearchAsync_SortWithType_DoesNotDependOnCurrentCulture(string culture)
    {
        // Under tr-TR, "string".ToUpper() is "STRİNG" (dotted capital I), which is not a SortFieldType name.
        using var directory = new RAMDirectory();
        using var analyzer = new StandardAnalyzer(LuceneConstants.DefaultVersion);

        using (var writer = new IndexWriter(directory, new IndexWriterConfig(LuceneConstants.DefaultVersion, analyzer)))
        {
            foreach (var title in new[] { "b", "c", "a" })
            {
                writer.AddDocument([new StringField("Title", title, Field.Store.YES)]);
            }
        }

        using var reader = DirectoryReader.Open(directory);
        var context = new LuceneQueryContext(new IndexSearcher(reader), LuceneConstants.DefaultVersion, analyzer);
        var queryService = new LuceneQueryService([new MatchAllQueryProvider()]);
        var query = JsonNode.Parse("""
        {
            "query": { "match_all": {} },
            "sort": { "Title": { "order": "asc", "type": "string" } }
        }
        """)!.AsObject();

        LuceneTopDocs result;
        using (CultureScope.Create(culture))
        {
            result = await queryService.SearchAsync(context, query);
        }

        var titles = result.TopDocs.ScoreDocs.Select(scoreDoc => context.IndexSearcher.Doc(scoreDoc.Doc).Get("Title"));
        Assert.Equal(["a", "b", "c"], titles);
    }
}
