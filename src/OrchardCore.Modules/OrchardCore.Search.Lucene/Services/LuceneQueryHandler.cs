using OrchardCore.Entities;
using OrchardCore.Queries;
using OrchardCore.Queries.Core;
using OrchardCore.Search.Lucene.Model;

namespace OrchardCore.Search.Lucene.Services;

public sealed class LuceneQueryHandler : QueryHandlerBase
{
    public override Task InitializingAsync(InitializingQueryContext context)
        => UpdateQueryAsync(context);

    public override Task UpdatingAsync(UpdatingQueryContext context)
        => UpdateQueryAsync(context);

    private static Task UpdateQueryAsync(DataQueryContextBase context)
    {
        if (context.Query.Source != LuceneQuerySource.SourceName)
        {
            return Task.CompletedTask;
        }

        var template = context.Data[nameof(LuceneQueryMetadata.Template)]?.GetValue<string>();
        var index = context.Data[nameof(LuceneQueryMetadata.Index)]?.GetValue<string>();

        var hasTemplate = !string.IsNullOrEmpty(template);
        var hasIndex = !string.IsNullOrEmpty(index);

        if (hasTemplate || hasIndex)
        {
            var metadata = context.Query.As<LuceneQueryMetadata>();

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
