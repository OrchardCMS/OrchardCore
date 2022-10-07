# Data (`OrchardCore.Data`)

## Configuring Databases

Most database configuration is handled automatically, but there are limited options which can affect the way the database works.

### Sqlite

#### `UseConnectionPooling` (boolean)

By default in `.NET 6`, `Microsoft.Data.Sqlite` pools connections to the database. It achieves this by putting locking the database file and leaving connections open to be reused. If the lock is preventing tasks like backups, this functionality can be disabled.

There may be a performance penalty associated with disabling connection pooling.

See the [`Microsoft.Data.Sqlite` documentation](https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/connection-strings#pooling) for more details.

##### `appsettings.json`

```json
{
    "OrchardCore_Data_Sqlite": {
        "UseConnectionPooling": false
    }
}
```

## Configuring YesSql

OrchardCore uses the `YesSql` library to interact with the configured database provider. `YesSql` is shipped with configuration that is suitable for most use cases. However, you can change these settings by configuring `YesSqlOptions`. `YesSqlOptions` provides the following configurable options

| Setting | Description |
| --- | --- |
| `CommandsPageSize` | Gets or sets the command page size. If you have to many queries in one command, `YesSql` will split the large command into multiple commands. |
| `ContentTypeOptions` | Gets or sets the `QueryGatingEnabled` option in `YesSql`. |
| `TableNameConvention` | You can provide your own implementation for generating ids. |
| `IdentifierAccessorFactory` | You can provide your own value accessor factory. |
| `VersionAccessorFactory` | You can provide your own version accessor factory. |
| `ContentSerializer` | You can provide your own content serializer. |
| `TablePrefixSeparator` | Gets or sets the prefix used to seperate database prefix and the table name. |

For example, you can change the default table prefix seperator from `_` to `__` by adding the following code to your startup code.

```C#
services.Configure<YesSqlOptions>(options =>
{
    options.TablePrefixSeparator = "__";
});
```

## Running SQL queries

### Creating a `DbConnection` instance

To get a new `DbConnection` pointing to the same database as the running site, use `IDbConnectionAccessor` from the `OrchardCore.Data` namespace in the `Orchard.Data.Abstractions` package..

### Writing database provider agnostic queries

Once a connection has been created, a custom `ISqlDialect` can be obtained from `IStore` from the `YesSql` namespace in the `YesSql.Abstractions` package.
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
    private readonly IStore _store;
    private readonly string _tablePrefix;

    public AdminController(IDbConnectionAccessor dbAccessor, IStore store, ShellSettings settings)
    {
        _dbAccessor = dbAccessor;
        _store = store;
        _tablePrefix = settings["TablePrefix"];
    }

    public async Task<ActionResult> Index()
    {
       using (var connection = _dbAccessor.CreateConnection())
       {
           using(var transaction = connection.BeginTransaction())
           {
                var dialect = _store.Configuration.SqlDialect;
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
