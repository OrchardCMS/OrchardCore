# Data (`OrchardCore.Data`)

## Running SQL queries

### Creating a `DbConnection` instance

To get a new `DbConnection` pointing to the same database as the running site, use `IDbConnectionAccessor` from the `OrchardCore.Data` namespace in the `Orchard.Data.Abstractions` package..

### Writing database provider agnostic queries

Once a connection has been created, a custom `ISqlDialect` can be obtained from `SqlDialectFactory.For(connection)` from the `YesSql` namespace in the `YesSql.Abstractions` package.
This service provides methods to build SQL queries that can will be use the syntax of the underlying connection.

### Handling prefixed tables

Each tenant in an Orchard Core application can have a table prefix. When building custom queries it 
is necessary to take it into account. It is available by resolving `ShellSettings` and accessing the `TablePrefix` setting.
It is available from the `OrchardCore.Environment.Shell` namespace in the `OrchardCore.Abstractions` package.

## Example

The following example uses Dapper to execute a SQL query.

```csharp
using Dapper;
using OrchardCore.Data;
using OrchardCore.Environment.Shell

public class AdminController : Controller
{
    private readonly IDbConnectionAccessor _dbAccessor;
    private readonly string _tablePrefix;

    public AdminController(IDbConnectionAccessor dbAccessor, ShellSettings settings)
    {
        _dbAccessor = dbAccessor;
        _tablePrefix = settings["TablePrefix"];
    }

    public async Task<ActionResult> Index()
    {
       using (var connection = _dbAccessor.CreateConnection())
       {
           using(var transaction = connection.BeginTransaction())
           {
                var dialect = SqlDialectFactory.For(connection);
                var customTable = dialect.QuoteForTableName($"{_tablePrefix}CustomTable");

                var selectCommand = $"SELECT * FROM {customTable}";

                var model = connection.QueryAsync<CustomTable>(selectCommand);

                // If an exception occurs the transaction is disposed and rollbacked
                transaction.Commit();

                return View(model);
            }
        }
    }
}
```

## Credits

### YesSQL

<https://github.com/sebastienros/yessql>

Copyright (c) 2017 Sebastien Ros  
MIT License