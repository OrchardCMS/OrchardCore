using OrchardCore.Entities;
using OrchardCore.Queries;
using OrchardCore.Queries.Core;
using OrchardCore.Search.Elasticsearch.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public sealed class ElasticsearchQueryHandler : QueryHandlerBase
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

        var template = context.Data[nameof(ElasticsearchQueryMetadata.Template)]?.GetValue<string>();
        var index = context.Data[nameof(ElasticsearchQueryMetadata.Index)]?.GetValue<string>();

        var hasTemplate = !string.IsNullOrEmpty(template);
        var hasIndex = !string.IsNullOrEmpty(index);

        if (hasTemplate || hasIndex)
        {
            var metadata = context.Query.As<ElasticsearchQueryMetadata>();

            if (hasTemplate)
            {
                metadata.Template = template;
            }

            if (hasIndex)
            {
                metadata.Index = index;
            }

            context.Query.Put(metadata);
        }

        context.Query.CanReturnContentItems = true;

        return Task.CompletedTask;
    }
}
