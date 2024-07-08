using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Queries.Core;
using OrchardCore.Queries.Sql.Models;

namespace OrchardCore.Queries.Sql;

public class SqlQueryHandler : QueryHandlerBase
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

        var metadata = new SqlQueryMetadata
        {
            Template = context.Data[nameof(SqlQueryMetadata.Template)]?.GetValue<string>()
        };

        context.Query.Put(metadata);
        context.Query.CanReturnContentItems = true;

        return Task.CompletedTask;
    }
}
