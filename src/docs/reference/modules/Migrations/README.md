# Data Migrations (`OrchardCore.Data.Migration.DataMigration`)

## Configuring Data Migration

Data Migration classes can be used to alter the content type definitions (like by adding new __types__, or configuring their __parts__ and __fields__),
initializing recipes or creating indices.

To implement a migration, your class should inherit from the DataMigration class. The initial migration method must be named `public int Create()` or `public Task<int> CreateAsync()`, and it should return a number (e.g., 1). Subsequent migrations should adhere to the convention public int `UpdateFromX()` or `public Task<int> UpdateFromXAsync()`, where `X` represents the number returned by the last migration method. The migration process strictly follows this chain: for instance, `UpdateFrom4()` will only execute if the last migration returned the number `4`. It is crucial not to modify these numbers afterward, as doing so may disrupt migrations for other tenants.

Migrations are executed automatically on application start.

The following example showcases three different data migrations (recipe migration, creating a map index, creating content type and updating content type).

```csharp
using System.Threading.Tasks;
using Members.Indexes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Recipes.Services;
using YesSql.Sql;

namespace Members;

public sealed class Migrations : DataMigration
{
    private readonly IRecipeMigrator _recipeMigrator;
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(
        IRecipeMigrator recipeMigrator, 
        IContentDefinitionManager contentDefinitionManager)
    {
        _recipeMigrator = recipeMigrator;
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _recipeMigrator.ExecuteAsync("init.recipe.json", this);

        return 1;
    }

    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<MemberIndex>(table =>
        {
            // Make sure to set column length, otherwise migration will not work for all databases
            table.Column<string>(nameof(MemberIndex.SocialSecurityNumber), column => column.WithLength(11))
            .Column<string>(nameof(MemberIndex.Name), column => column.WithLength(26))
            .Column<string>(nameof(MemberIndex.Surname), column => column.WithLength(26))
        });
            
        // This will create index on the sql table itself, 
        // which will make fetches by 'SocialSecurityNumber' column faster
        await SchemaBuilder.AlterIndexTableAsync<MemberIndex>(table => table
            .CreateIndex("IDX_MemberIndex_SocialSecurityNumber",
                "SocialSecurityNumber")
        );
        return 2;
    }

    public Task<int> UpdateFrom2Async()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync("Product", type => type
            // Content items of this type can have drafts
            .Draftable()
            // Content items versions of this type are versionable
            .Versionable()
            // This content type appears in the New menu section
            .Creatable()
            // Permissions can be applied specifically to instances of this type
            .Securable()
        );
        return 3;
    }

    public Task<int> UpdateFrom3Async()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync("Product", type => type
            .WithPart("TitlePart")
        );
        return 4;
    }
}
```

## Indexes added by update steps

Creating an index on a large table can take a long time, and on most databases it blocks the writes to the table meanwhile. As migrations run when a tenant starts, an update step that adds an index to a large table, such as `ContentItemIndex`, can keep the site from responding during the upgrade.

The `OrchardCore:Data:Migrations:CreateIndexesInUpdates` setting defines whether the update steps create the indexes they add to existing tables:

| Value | Description |
| --- | --- |
| `All` | The default. The update steps create their indexes in every environment. |
| `Development` | The update steps create their indexes only when the application runs in the `Development` environment. |
| `Disabled` | The update steps never create their indexes. |

```json
{
  "OrchardCore": {
    "Data": {
      "Migrations": {
        "CreateIndexesInUpdates": "Development"
      }
    }
  }
}
```

The `Create` step of a migration, and the update steps that run right after it, always create their indexes, as the tables are new and empty then. Only the indexes added to a table that existed before the step are affected: the other changes of the step, such as new columns, new tables and the indexes dropped from a table, are applied in every case.

When the update steps of a migration run without creating their indexes, a warning names the migration and the version it was updated from. The migration still records its new version, so it won't create these indexes later: create them with the SQL statements of the [release notes](#indexes-of-each-release), when the load of the database allows it, or leave them out if your queries don't need them.

### Creating the indexes

The statements of the release notes use the online options of each database, which keep the table available while the index is built:

- **SQL Server:** `ONLINE = ON` is only available in the Enterprise and Developer editions, and in Azure SQL. In other editions, remove `WITH (ONLINE = ON)`, which blocks the writes to the table while the index is built.
- **PostgreSQL:** `CREATE INDEX CONCURRENTLY` can't run in a transaction, so run each statement on its own, not in a single `psql -c` command. If it fails, it leaves an invalid index: drop it and run the statement again.
- **MySQL:** `ALGORITHM=INPLACE LOCK=NONE` builds the index without blocking the writes, or fails instead of blocking them. `DROP INDEX` fails when the index doesn't exist: skip the statement then.
- **SQLite:** there is no online option. The database is locked while the index is built.

Adapt the names to your tenant before running the statements:

- **Table prefix:** a tenant with a table prefix, such as `tenant1`, has its tables named with the prefix and the table name separator, e.g. `tenant1_ContentItemIndex`.
- **Schema:** a tenant with a schema has its tables in that schema, e.g. `[orchard].[ContentItemIndex]` on SQL Server.
- **Collections:** a table in a collection is named with the collection and the separator, e.g. `Notification_NotificationIndex`.
- **Index names on PostgreSQL:** the index names start with the table prefix too, e.g. `tenant1_IDX_ContentItemIndex_ContentType`, as PostgreSQL requires unique index names in a schema. A name of 63 characters or more, prefix included, is replaced with a hash, and you can use any name then.

Keep the index names: a later migration may drop or replace an index by its name.

### Indexes of each release

The release notes of each version list the indexes added by its update steps, with the SQL statements to create them:

- [Orchard Core 4.0.0](../../../releases/4.0.0.md#indexes-created-by-migration-update-steps)

## Additional information

Please refer to separate sections for more details on data migrations:

- [Data Migration of Content Types](../ContentTypes/README.md#migrations)
- [Data Migration of Recipes](../Recipes/README.md#recipe-migrations)

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/bayT58i7DVY" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/mN4H6hIBzWI" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>
