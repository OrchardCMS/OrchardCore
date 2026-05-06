using OrchardCore.Entities;
using OrchardCore.Queries;
using OrchardCore.Queries.Core;
using OrchardCore.OpenSearch.Models;

namespace OrchardCore.OpenSearch.Core.Services;

public sealed class OpenSearchQueryHandler : QueryHandlerBase
{
    public override Task InitializingAsync(InitializingQueryContext context)
        => UpdateQueryAsync(context);

    public override Task UpdatingAsync(UpdatingQueryContext context)
        => UpdateQueryAsync(context);

    private static Task UpdateQueryAsync(DataQueryContextBase context)
    {
        if (context.Query.Source != OpenSearchQuerySource.SourceName)
        {
            return Task.CompletedTask;
        }

        var template = context.Data[nameof(OpenSearchQueryMetadata.Template)]?.GetValue<string>();
        var index = context.Data[nameof(OpenSearchQueryMetadata.Index)]?.GetValue<string>();

        var hasTemplate = !string.IsNullOrEmpty(template);
        var hasIndex = !string.IsNullOrEmpty(index);

        if (hasTemplate || hasIndex)
        {
            var metadata = context.Query.As<OpenSearchQueryMetadata>();

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

        return Task.CompletedTask;
    }
}
