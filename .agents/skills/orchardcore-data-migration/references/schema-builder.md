# SchemaBuilder reference

`SchemaBuilder` is supplied by the `DataMigration` base class (`SchemaBuilder` property) — never inject it. It builds and alters the SQL **index tables** that back YesSql map/reduce indexes.

All methods have async forms (`...Async`); prefer them in new code.

## Create a map index table

```csharp
await SchemaBuilder.CreateMapIndexTableAsync<MemberIndex>(table => table
    .Column<string>("ContentItemId", column => column.WithLength(26))
    .Column<string>(nameof(MemberIndex.SocialSecurityNumber), column => column.WithLength(11))
    .Column<string>(nameof(MemberIndex.Name), column => column.WithLength(26))
    .Column<bool>("Published", column => column.Nullable())
    .Column<decimal>("Amount", column => column.Nullable())
    .Column<string>("BigText", column => column.Nullable().Unlimited())
);
```

The index POCO (`MemberIndex : MapIndex`) and its `IndexProvider` are defined separately in the module; the migration only creates the **table**.

## Column rules

| Need | Code |
|------|------|
| String with length | `.Column<string>("Name", c => c.WithLength(26))` |
| Unbounded text | `.Column<string>("Body", c => c.Nullable().Unlimited())` |
| Nullable value | `.Column<bool>("Latest", c => c.Nullable())` |
| Numeric | `.Column<decimal>(...)`, `.Column<int>(...)`, `.Column<DateTime>(...)`, `.Column<TimeSpan>(...)` |

**Always** give string columns an explicit `.WithLength(n)` or `.Unlimited()`. An unlengthed string column defaults inconsistently across providers and breaks the migration on some databases. Standard OrchardCore lengths: `ContentItemId` / `ContentItemVersionId` = 26, content type/part/field names use the `ContentItemIndex.Max*Size` constants.

## Alter a table: add / drop columns and indexes

```csharp
// Add a column in an upgrade step
await SchemaBuilder.AlterIndexTableAsync<LinkFieldIndex>(table => table
    .AddColumn<string>("BigUrl", column => column.Nullable().Unlimited()));

// Create a SQL index for faster lookups
await SchemaBuilder.AlterIndexTableAsync<LinkFieldIndex>(table => table
    .CreateIndex("IDX_LinkFieldIndex_DocumentId",
        "DocumentId",
        "ContentItemId",
        "ContentItemVersionId",
        "Published",
        "Latest"));

// Drop an index
await SchemaBuilder.AlterIndexTableAsync<TimeFieldIndex>(table => table
    .DropIndex("IDX_TimeFieldIndex_DocumentId_Time"));
```

## Provider quirks

### SQLite cannot drop columns

Wrap column drops (and the column rebuild that follows) in `try/catch`:

```csharp
try
{
    await SchemaBuilder.AlterIndexTableAsync<TimeFieldIndex>(table => table
        .DropColumn("Time"));

    await SchemaBuilder.AlterIndexTableAsync<TimeFieldIndex>(table => table
        .AddColumn<TimeSpan>("Time", column => column.Nullable()));
}
catch
{
    _logger.LogWarning("Failed to alter 'Time' column. This is not an error when using SqLite");
}
```

### Dropping an index that may not exist

```csharp
try
{
    await SchemaBuilder.AlterIndexTableAsync<TimeFieldIndex>(table => table
        .DropIndex("IDX_TimeFieldIndex_Time"));
}
catch
{
    _logger.LogWarning("Failed to drop an index that does not exist 'IDX_TimeFieldIndex_Time'");
}
```

### MySQL index key-length limit

A composite index key in MySQL is capped at 768 chars / 3072 bytes. When indexing long string columns, prefix-limit them in the index definition:

```csharp
await SchemaBuilder.AlterIndexTableAsync<TextFieldIndex>(table => table
    .CreateIndex("IDX_TextFieldIndex_DocumentId_Text",
        "DocumentId",
        "Text(764)",   // prefix length keeps the key under the limit
        "Published",
        "Latest"));
```

OrchardCore documents the arithmetic in comments above each such index, e.g.
`// DocumentId (2) + Text (764) + Published and Latest (1) = 767 (< 768).`

## Drop a whole table

```csharp
await SchemaBuilder.DropMapIndexTableAsync<MemberIndex>();
// or
await SchemaBuilder.DropTableAsync("MyCustomTable");
```

Typically used in `UninstallAsync()`.
