using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Queries;
using OrchardCore.Queries.Core;
using OrchardCore.Search.Elasticsearch.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public class ElasticsearchQueryHandler : QueryHandlerBase
{
    public override Task InitializingAsync(InitializingQueryContext context)
        => UpdateQueryAsync(context);

    public override Task UpdatingAsync(UpdatingQueryContext context)
        => UpdateQueryAsync(context);

    private static Task UpdateQueryAsync(DataQueryContextBase context)
    {
        if (context.Query.Source == ElasticQuerySource.SourceName)
        {
            return Task.CompletedTask;
        }

        var metadata = new ElasticsearchQueryMetadata
        {
            Template = context.Data[nameof(ElasticsearchQueryMetadata.Template)]?.GetValue<string>(),
            Index = context.Data[nameof(ElasticsearchQueryMetadata.Index)]?.GetValue<string>()
        };

        context.Query.Put(metadata);
        context.Query.CanReturnContentItems = true;

        return Task.CompletedTask;
    }
}
