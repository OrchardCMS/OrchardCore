# Data (`OrchardCore.Data`)

Orchard Core uses [YesSql](https://github.com/sebastienros/yessql) to store documents and indexes in a relational database. The Data services configure YesSql for each tenant and provide access to the tenant's database connection, SQL dialect, schema, and table naming conventions.

For an introduction to documents, indexes, and sessions, see [How YesSql works](../../../topics/data/yessql.md).

## Database providers

You select a database provider during tenant setup. Orchard Core includes these providers:

| Database | Provider value | Connection string | Table prefix and schema |
| --- | --- | --- | --- |
| SQLite | `Sqlite` | Not used | Not used |
| SQL Server | `SqlConnection` | Required | Supported |
| MySQL | `MySql` | Required | Supported |
| PostgreSQL | `Postgres` | Required | Supported |

For SQL Server, MySQL, and PostgreSQL, create the database before running setup and grant the configured account permission to create and modify tables. Orchard Core validates the connection and then creates its tables; it doesn't create the database itself.

The provider, connection string, table prefix, and schema are tenant settings. You can enter them on the setup screen or preconfigure them through `IShellConfiguration`. See [Setup](../Setup/README.md) for setup parameters and [Configuration](../Configuration/README.md) for tenant configuration sources and shared-database guidance.

## SQLite options

SQLite is the default provider and stores each tenant's database file in that tenant's shell data directory.

### Database name

The `DatabaseName` shell setting controls the file name. Setup uses `OrchardCore.db` when no name is provided. Because the value belongs to the tenant's `ShellSettings`, each tenant can use a different file name.

### Connection pooling

`Microsoft.Data.Sqlite` connection pooling is enabled by default. Pooled connections can keep the database file open, which may interfere with operations such as copying or replacing the file for a backup. Set `UseConnectionPooling` to `false` when you need those operations to release the file; disabling pooling can reduce performance.

In the root web application's `appsettings.json`, configure the option under the `OrchardCore` section:

```json
{
  "OrchardCore": {
    "OrchardCore_Data_Sqlite": {
      "UseConnectionPooling": false
    }
  }
}
```

See the [`Microsoft.Data.Sqlite` connection string documentation](https://learn.microsoft.com/dotnet/standard/data/sqlite/connection-strings#pooling) for details about pooling.

## YesSql options

Orchard Core binds the `OrchardCore_YesSql` configuration section to `YesSqlOptions`.

| Setting | Default | Description |
| --- | --- | --- |
| `CommandsPageSize` | `500` | Sets the maximum number of commands in a YesSql command page. YesSql splits larger sets into multiple pages. |
| `QueryGatingEnabled` | `true` | Coalesces identical concurrent query work so YesSql executes it once and shares the result. |
| `EnableThreadSafetyChecks` | `false` | Enables checks that help diagnose concurrent use of a YesSql session. |
| `IsolationLevel` | `ReadCommitted` | Sets the default transaction isolation level passed to the configured provider. |

Configure these values through any supported tenant configuration source. For example:

```json
{
  "OrchardCore": {
    "OrchardCore_YesSql": {
      "CommandsPageSize": 1000,
      "QueryGatingEnabled": true,
      "EnableThreadSafetyChecks": false,
      "IsolationLevel": "ReadCommitted"
    }
  }
}
```

`YesSqlOptions` also exposes `IdGenerator`, `IdentifierAccessorFactory`, `VersionAccessorFactory`, and `ContentSerializer` for service implementations that can't be created by configuration binding. Configure those options in code:

```csharp
using OrchardCore.Data.YesSql;

services.Configure<YesSqlOptions>(options =>
{
    options.CommandsPageSize = 1000;
});
```

## Table naming presets

The `OrchardCore_Data_TableOptions` section defines presets that Orchard Core copies into a tenant's shell settings during initial setup.

| Setting | Default for a new tenant | Description |
| --- | --- | --- |
| `DefaultDocumentTable` | `Document` | Sets the name of the default YesSql document table. |
| `DefaultTableNameSeparator` | `_` | Sets the separator between a table prefix or collection name and the table name. Use one or more underscores, or `NULL` for no separator. |
| `DefaultIdentityColumnSize` | `Int64` | Sets identity columns to `Int32` or `Int64`. |

```json
{
  "OrchardCore": {
    "OrchardCore_Data_TableOptions": {
      "DefaultDocumentTable": "Document",
      "DefaultTableNameSeparator": "_",
      "DefaultIdentityColumnSize": "Int64"
    }
  }
}
```

!!! warning
    Configure these presets before setting up a tenant. Changing them later doesn't rename existing tables or alter existing identity columns.

The examples above show the root web application's `appsettings.json` shape. In a tenant-local `App_Data/Sites/{tenant}/appsettings.json` file, omit the outer `OrchardCore` section. See [Configuration](../Configuration/README.md) for the complete configuration source hierarchy.

## Running SQL queries

Prefer `IContentManager` for content items and `ISession` for YesSql documents and indexes. Use raw SQL only when you need to work directly with relational tables.

`IDbConnectionAccessor` from the `OrchardCore.Data` namespace creates a `DbConnection` for the current tenant. The interface is provided by the `OrchardCore.Data.Abstractions` package. Resolve `IStore` from the `YesSql` namespace to access the configured SQL dialect, schema, and table prefix.

Create custom relational tables through a [data migration](../Migrations/README.md) so Orchard Core can apply their schema consistently.

### Quote table names

Database providers use different identifier syntax. Build table names from the YesSql store configuration and quote them with `ISqlDialect`:

```csharp
using Dapper;
using OrchardCore.Data;
using YesSql;

public sealed class CustomTableReader
{
    private readonly IDbConnectionAccessor _dbConnectionAccessor;
    private readonly IStore _store;

    public CustomTableReader(IDbConnectionAccessor dbConnectionAccessor, IStore store)
    {
        _dbConnectionAccessor = dbConnectionAccessor;
        _store = store;
    }

    public async Task<IReadOnlyList<CustomRow>> ListAsync(
        CancellationToken cancellationToken)
    {
        await using var connection = _dbConnectionAccessor.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var configuration = _store.Configuration;
        var tableName = configuration.SqlDialect.QuoteForTableName(
            $"{configuration.TablePrefix}CustomTable",
            configuration.Schema);

        var command = new CommandDefinition(
            $"SELECT * FROM {tableName};",
            cancellationToken: cancellationToken);

        return (await connection.QueryAsync<CustomRow>(command)).AsList();
    }
}
```

`IStore.Configuration.TablePrefix` already includes the configured table-name separator. Pass `IStore.Configuration.Schema` when quoting a table so the query also works for tenants that use a non-default schema.

Table names can't be supplied as SQL parameters, so only compose identifiers from trusted application and tenant configuration. Pass data values to Dapper as parameters instead of interpolating them into SQL.

### Use transactions for related writes

Open the connection before beginning a transaction, pass the transaction to every Dapper command, and rethrow failures after rollback:

```csharp
await using var connection = _dbConnectionAccessor.CreateConnection();
await connection.OpenAsync(cancellationToken);
await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

try
{
    await connection.ExecuteAsync(new CommandDefinition(
        firstCommand,
        transaction: transaction,
        cancellationToken: cancellationToken));

    await connection.ExecuteAsync(new CommandDefinition(
        secondCommand,
        transaction: transaction,
        cancellationToken: cancellationToken));

    await transaction.CommitAsync(cancellationToken);
}
catch
{
    await transaction.RollbackAsync(cancellationToken);
    throw;
}
```
