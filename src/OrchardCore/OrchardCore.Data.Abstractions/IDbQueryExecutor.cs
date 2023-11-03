using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace OrchardCore.Data;

public interface IDbQueryExecutor
{
    Task ExecuteAsync(Func<DbConnection, DbTransaction, Task> callback, DbExecutionContext context);

    Task QueryAsync(Func<DbConnection, Task> callback, DbQueryContext context);
}
