using OrchardCore.Entities;
using OrchardCore.Queries.Core;
using OrchardCore.Queries.Sql.Models;

namespace OrchardCore.Queries.Sql;

public sealed class SqlQueryHandler : QueryHandlerBase
{
    public override Task InitializingAsync(InitializingQueryContext context)
        => UpdateQueryAsync(context);

    public override Task UpdatingAsync(UpdatingQueryContext context)
        => UpdateQueryAsync(context);

    private static Task UpdateQueryAsync(DataQueryContextBase context)
    {
        if (context.Query.Source != SqlQuerySource.SourceName)
        {
            return Task.CompletedTask;
        }

        var template = context.Data[nameof(SqlQueryMetadata.Template)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(template))
        {
            var metadata = context.Query.As<SqlQueryMetadata>();

            metadata.Template = template;
            context.Query.Put(metadata);
        };

        context.Query.CanReturnContentItems = true;

        return Task.CompletedTask;
    }
}
