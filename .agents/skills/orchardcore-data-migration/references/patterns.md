# Migration patterns

Common, ready-to-adapt migration shapes drawn from OrchardCore modules.

## Content definition: create a part

```csharp
public async Task<int> CreateAsync()
{
    await _contentDefinitionManager.AlterPartDefinitionAsync("HtmlBodyPart", builder => builder
        .Attachable()
        .WithDescription("Provides an HTML Body for your content item."));

    return 6; // latest version; new installs skip every UpdateFromX
}
```

## Content definition: create a type

```csharp
await _contentDefinitionManager.AlterTypeDefinitionAsync("Product", type => type
    .Creatable()
    .Listable()
    .Draftable()
    .Versionable()
    .Securable()
    .WithPart("TitlePart")
    .WithPart("ProductPart"));
```

## Content definition: replace a part on every type

Used when renaming/replacing a part across all content types (e.g. `BodyPart` → `HtmlBodyPart`):

```csharp
public async Task<int> UpdateFrom3Async()
{
    foreach (var contentType in await _contentDefinitionManager.LoadTypeDefinitionsAsync())
    {
        if (contentType.Parts.Any(x => x.PartDefinition.Name == "BodyPart"))
        {
            await _contentDefinitionManager.AlterTypeDefinitionAsync(contentType.Name,
                x => x.RemovePart("BodyPart").WithPart("HtmlBodyPart"));
        }
    }

    await _contentDefinitionManager.DeletePartDefinitionAsync("BodyPart");

    return 4;
}
```

Use `LoadTypeDefinitionsAsync()` (mutable, for writing) inside migrations, not `ListTypeDefinitionsAsync()` (cached read).

## Content definition: migrate part settings

```csharp
public async Task<int> UpdateFrom5Async()
{
    foreach (var contentType in await _contentDefinitionManager.LoadTypeDefinitionsAsync())
    {
        if (contentType.Parts.Any(p => p.PartDefinition.Name == "HtmlBodyPart"))
        {
            await _contentDefinitionManager.AlterTypeDefinitionAsync(contentType.Name, t => t.WithPart("HtmlBodyPart", part =>
            {
                part.MergeSettings<HtmlBodyPartSettings>(s => s.RenderLiquid = !s.SanitizeHtml);
            }));
        }
    }

    return 6;
}
```

## Patch existing content item data

Move/transform data already stored in content items. Page by `DocumentId`, save changed items, flush each page. This step is **not** needed on a fresh install (no items exist yet), so it lives in an `UpdateFromX`, never in `CreateAsync`.

```csharp
public async Task<int> UpdateFrom1Async()
{
    var lastDocumentId = 0L;

    for (; ; )
    {
        var contentItemVersions = await _session
            .Query<ContentItem, ContentItemIndex>(x => x.DocumentId > lastDocumentId)
            .Take(10)
            .ListAsync();

        if (!contentItemVersions.Any())
        {
            break; // no more to process
        }

        foreach (var contentItemVersion in contentItemVersions)
        {
            if (UpdateItem((JsonObject)contentItemVersion.Content))
            {
                await _session.SaveAsync(contentItemVersion);
            }

            lastDocumentId = contentItemVersion.Id;
        }

        await _session.FlushAsync();
    }

    return 2;
}
```

Notes:
- Query the index (`ContentItemIndex`) but iterate the document (`ContentItem`); `contentItemVersion.Id` is the YesSql `DocumentId` cursor.
- Mutate `contentItemVersion.Content` as a `System.Text.Json` `JsonObject`/`JsonNode`, then `SaveAsync`.
- Iterating **all** versions (not just latest/published) patches drafts and history too.
- Recurse into nested parts (e.g. inside Flows/Bag parts) — content can be deeply nested.

## Run a recipe from a migration

Run a setup recipe (relative to the module's `Migrations` location) — handy for seeding templates, queries, layers, or sample content.

```csharp
public Migrations(IRecipeMigrator recipeMigrator) => _recipeMigrator = recipeMigrator;

public async Task<int> CreateAsync()
{
    await _recipeMigrator.ExecuteAsync("init.recipe.json", this);

    return 1;
}
```

The recipe file ships with the module (typically under a `Migrations/` or `Recipes/` folder) and uses the same recipe step format as setup recipes — see the `orchardcore-recipe-creator` skill.

## Skipping versions intentionally

Return a higher number than `X+1` when a later step makes an intermediate one obsolete:

```csharp
public async Task<int> UpdateFrom3Async()
{
    // ...
    return 5; // Returning 5 instead of 4 because UpdateFrom4 is no longer needed.
}
```

The runner then looks for `UpdateFrom5Async` next, never calling the removed `UpdateFrom4Async`.
