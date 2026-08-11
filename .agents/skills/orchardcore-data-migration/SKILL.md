---
name: orchardcore-data-migration
description: Creates and updates OrchardCore data migrations (DataMigration classes with CreateAsync/UpdateFromX). Use when the user needs to alter content type definitions, create or change SQL index tables, patch existing content items, run a recipe migration, or version a module's schema.
---

# OrchardCore Data Migration

This skill guides you through writing OrchardCore data migrations following project conventions.

A data migration is a class that inherits `DataMigration`. OrchardCore discovers its methods by reflection and runs them **sequentially**, per tenant, on application start. The chain starts at `CreateAsync()` and continues through `UpdateFrom1Async()`, `UpdateFrom2Async()`, … Each method returns the schema version number the **next** method must match.

## When to use a migration

- Define or alter content types / parts / fields (`IContentDefinitionManager`).
- Create or alter SQL index tables (`SchemaBuilder`).
- Patch existing content item data (query + `ISession.SaveAsync`).
- Run a setup recipe at install/upgrade (`IRecipeMigrator`).
- Seed roles, settings, or other documents.

## The version-chain rule (read first)

The migration runner stores the **last returned number** per migration class per tenant. It then calls the method whose name matches: returned `4` → runs `UpdateFrom4Async()`. The method's return value is the new stored version.

| Rule | Why |
|------|-----|
| `CreateAsync` runs only on a **fresh** install. | Existing tenants already passed it. |
| `CreateAsync` returns the **current latest** version, skipping all `UpdateFromX`. | New sites get the final schema directly. |
| `UpdateFromXAsync` runs only when stored version == `X`. | Sequential upgrade path. |
| **Never renumber** an already-shipped method. | Other tenants store the old number; renumbering breaks their upgrade. |
| Each `UpdateFromX` returns `X+1` (usually). | Skipping is allowed when a later step subsumes an earlier one (return a higher number to jump). |

## Migration Workflow

### Step 1: Create the Migrations class

Place `Migrations.cs` in the module root (or a `Migrations/` subfolder for multiple classes, e.g. `Migrations/PermissionMigrations.cs`).

```csharp
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;

namespace YourModule;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        // ... define schema ...

        // Return the latest version so new installs skip every UpdateFromX.
        return 1;
    }
}
```

`SchemaBuilder` is injected by the base class — do **not** add it as a constructor parameter.

### Step 2: Register the migration

In the module `Startup.cs`:

```csharp
services.AddDataMigration<Migrations>();
```

Register each migration class separately if the module has more than one.

### Step 3: Write the initial `CreateAsync`

Return the highest version your module currently ships. Example (content part):

```csharp
public async Task<int> CreateAsync()
{
    await _contentDefinitionManager.AlterPartDefinitionAsync("TitlePart", builder => builder
        .Attachable()
        .WithDescription("Provides a Title for your content item."));

    // Shortcut other migration steps on new content definition schemas.
    return 2;
}
```

### Step 4: Add `UpdateFromX` for each shipped change

When you change the schema in a release, **append** a new `UpdateFromXAsync` — never edit a prior one. Bump the number `CreateAsync` returns to match.

```csharp
// This code can be removed in a later version.
public async Task<int> UpdateFrom1Async()
{
    await SchemaBuilder.AlterIndexTableAsync<LinkFieldIndex>(table => table
        .AddColumn<string>("BigUrl", column => column.Nullable().Unlimited()));

    return 2;
}
```

### Step 5: Test the migration

1. **Fresh install** — run on a clean tenant; only `CreateAsync` executes, schema lands at the latest version.
2. **Upgrade** — start from a DB at the previous version; confirm each `UpdateFromX` runs in order.
3. Test on every targeted provider (SQLite, SQL Server, MySQL, PostgreSQL). SQLite cannot drop columns — guard with `try/catch` (see `references/schema-builder.md`).

## Method signatures

| Method | Forms | Runs when |
|--------|-------|-----------|
| Create | `int Create()` · `Task<int> CreateAsync()` | fresh install |
| Update | `int UpdateFromX()` · `Task<int> UpdateFromXAsync()` | stored version == X |
| Uninstall | `void Uninstall()` · `Task UninstallAsync()` | feature uninstalled |

Static or instance, sync or async — all supported. Async (`...Async`) is the convention for new code.

## Quick Reference

### Common operations

| Goal | API |
|------|-----|
| Define/alter content type | `_contentDefinitionManager.AlterTypeDefinitionAsync(...)` |
| Define/alter part | `_contentDefinitionManager.AlterPartDefinitionAsync(...)` |
| Remove part / type | `RemovePart(...)`, `DeletePartDefinitionAsync(...)` |
| Create SQL index table | `SchemaBuilder.CreateMapIndexTableAsync<TIndex>(...)` |
| Add/drop column or index | `SchemaBuilder.AlterIndexTableAsync<TIndex>(...)` |
| Run a recipe | `_recipeMigrator.ExecuteAsync("init.recipe.json", this)` |
| Patch content items | `ISession.Query<ContentItem, ContentItemIndex>(...)` + `SaveAsync` |

### Type definition builder methods

| Method | Effect |
|--------|--------|
| `.Creatable()` | Appears in the New menu |
| `.Listable()` | Shows in content list |
| `.Draftable()` | Supports drafts |
| `.Versionable()` | Keeps version history |
| `.Securable()` | Per-type permissions |
| `.WithPart("PartName")` | Attaches a part |

## Gotchas

- Always set column **length** on string index columns (`.WithLength(n)`) or `.Unlimited()` — unlengthed columns fail on some providers.
- `CreateAsync` should return the latest version, not `1`, once `UpdateFromX` methods exist.
- Patch loops must page by `DocumentId` and call `FlushAsync()` periodically (see `references/patterns.md`).
- Mark obsolete `UpdateFromX` methods with `// This code can be removed in a later version.` but keep them until you drop support for that upgrade path.

## References

- `references/patterns.md` — content definition changes, content-item data patching, recipe migrations
- `references/schema-builder.md` — index tables, columns, indexes, provider quirks
- `src/docs/reference/modules/Migrations/README.md` (repo) — official reference
- `AGENTS.md` (repo root) — build commands
