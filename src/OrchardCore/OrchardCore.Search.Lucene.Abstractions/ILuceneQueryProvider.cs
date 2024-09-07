using System.Text.Json.Nodes;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene;

public interface ILuceneQueryProvider
{
    Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonObject query);
}
