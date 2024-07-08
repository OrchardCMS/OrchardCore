using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Queries;
using OrchardCore.Queries.Core;
using OrchardCore.Search.Lucene.Model;

namespace OrchardCore.Search.Lucene.Services;

public class LuceneQueryHandler : QueryHandlerBase
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

        var metadata = new LuceneQueryMetadata
        {
            Template = context.Data[nameof(LuceneQueryMetadata.Template)]?.GetValue<string>(),
            Index = context.Data[nameof(LuceneQueryMetadata.Index)]?.GetValue<string>()
        };

        context.Query.Put(metadata);
        context.Query.CanReturnContentItems = true;

        return Task.CompletedTask;
    }
}
