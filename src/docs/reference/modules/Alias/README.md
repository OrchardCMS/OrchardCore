# Alias (`OrchardCore.Alias`)

The Alias module adds `AliasPart`, which gives a content item a stable logical identifier such as `main-menu` or `footer-widget`.
Aliases are useful when code, templates, recipes, and deployment plans need to find content without relying on environment-specific content item IDs.

An alias is not a URL. Use [`AutoroutePart`](../Autoroute/README.md) when a content item also needs a route.

## Enable the feature

Enable the **Alias** feature (`OrchardCore.Alias`) under **Configuration** > **Features**.
The feature depends on `OrchardCore.ContentTypes`.

To enable it from a recipe:

```json
{
  "steps": [
    {
      "name": "Feature",
      "enable": [
        "OrchardCore.Alias"
      ]
    }
  ]
}
```

## Attach and configure `AliasPart`

In the admin:

1. Go to **Content** > **Content Definition** > **Content Types**.
2. Edit a content type and select **Add Parts**.
3. Add **AliasPart**.
4. Edit the part settings to configure its **Options** and **Pattern**.

The available options are:

| Option | Behavior |
| --- | --- |
| **Alias is editable** | Editors can enter an alias. If they leave it empty and a pattern is configured, Orchard Core generates the alias. |
| **Alias is generated and input is disabled** | The editor displays the alias as a disabled input. Orchard Core generates it from the pattern when the alias is empty. |

The default Liquid pattern is:

```liquid
{{ Model.ContentItem.DisplayText | slugify }}
```

The `ContentItem` global is also available to the pattern. For example:

```liquid
{{ ContentItem | display_text | slugify }}
```

!!! important
    A pattern is evaluated only when the current alias is empty. Changing the display text or the pattern does not replace an existing alias. With the editable option, clear the alias and save the item to generate it again.

### Define the part in a recipe

The following `ContentDefinition` step attaches `AliasPart` to an `Article` content type and prevents editors from changing the generated value:

```json
{
  "steps": [
    {
      "name": "ContentDefinition",
      "ContentTypes": [
        {
          "Name": "Article",
          "DisplayName": "Article",
          "Settings": {
            "ContentTypeSettings": {
              "Creatable": true,
              "Draftable": true,
              "Versionable": true
            }
          },
          "ContentTypePartDefinitionRecords": [
            {
              "PartName": "AliasPart",
              "Name": "AliasPart",
              "Settings": {
                "AliasPartSettings": {
                  "Pattern": "{{ Model.ContentItem | display_text | slugify }}",
                  "Options": "GeneratedDisabled"
                },
                "ContentTypePartSettings": {
                  "Position": "0"
                }
              }
            }
          ]
        }
      ]
    }
  ]
}
```

Use `"Editable"` for `Options` when editors may override the generated value.

### Import content with an alias

Aliases are part of the content item data, so content recipes and deployment plans preserve them:

```json
{
  "steps": [
    {
      "name": "content",
      "Data": [
        {
          "ContentItemId": "[js: uuid()]",
          "ContentType": "Article",
          "DisplayText": "Support",
          "Latest": true,
          "Published": true,
          "AliasPart": {
            "Alias": "support"
          }
        }
      ]
    }
  ]
}
```

## Alias rules and generation

- An alias can remain empty when neither an editor value nor a generation pattern supplies one. An empty alias is not indexed and cannot be resolved.
- An alias can contain up to 735 characters.
- Orchard Core does not automatically trim or slugify an alias entered by an editor or imported with content. Apply `slugify` in the generation pattern when a slug-like value is required.
- Generated aliases have line endings removed and are truncated to the maximum length.
- Non-empty aliases are validated for uniqueness across all content types in the tenant. The current content item is excluded from its own uniqueness check.
- When a generated alias already exists, Orchard Core appends a numeric suffix such as `-1`, trimming the base when necessary to remain within the length limit.
- Cloning a content item generates a suffixed alias for the clone.
- Alias lookup is case-insensitive: the YesSql index stores aliases in lowercase and lookup values are normalized to lowercase. Treat aliases as case-insensitively unique and do not assign values that differ only by letter casing.

Uniqueness is validated through `AliasPartIndex`; the database schema does not add a unique constraint. Applications that create content concurrently should still handle content validation failures and should not deliberately assign the same alias from multiple operations.

## Content lifecycle

`AliasPart` participates in the normal content lifecycle:

| Event | Behavior |
| --- | --- |
| Create or update | If the alias is empty, the configured Liquid pattern is rendered and a unique alias is generated. |
| Validate | Non-empty aliases are checked for the length limit and uniqueness. |
| Clone | The clone receives a unique, numerically suffixed alias. |
| Publish or unpublish | The `alias:{alias}` dynamic-cache tag is invalidated. |
| Remove | The cache tag is invalidated when no active version remains. |
| Remove `AliasPart` from the content type | On the next content update, the stale part data and its map-index entry are removed. |

Both the latest and published versions can contribute rows to `AliasPartIndex`. APIs that resolve an alias first obtain a content item ID; the caller then chooses whether to load the published or latest version.

## Look up content by alias

The standard alias handle format is:

```text
alias:main-menu
```

The `alias:` prefix is case-insensitive. The alias value is also matched case-insensitively.

### Liquid

Use the general `Content` accessor with an alias handle to load the published version:

```liquid
{% assign menu = Content["alias:main-menu"] %}
```

Use `Content.Latest` when a template must load the latest version, including a draft:

```liquid
{% assign menu = Content.Latest["alias:main-menu"] %}
```

The Alias module also provides a published-only convenience accessor:

```liquid
{% assign menu = Content.Alias["main-menu"] %}
```

When no matching content item is found, these expressions return `nil`.

To render a content item by its alias handle:

```liquid
{% contentitem handle: "alias:main-menu", display_type: "Summary" %}
```

The module registers these Liquid accessors but does not add an alias-specific JavaScript scripting function.

### Razor

Alias-specific helper methods accept an alias with or without the `alias:` prefix:

```csharp
@{
    var contentItemId = await Orchard.GetContentItemIdByAliasAsync("main-menu");
    var publishedItem = await Orchard.GetContentItemByAliasAsync("main-menu");
    var latestItem = await Orchard.GetContentItemByAliasAsync("main-menu", latest: true);
}
```

The generic content-handle helpers are useful when an application accepts more than one handle type:

```csharp
@using OrchardCore.ContentManagement

@{
    var item = await Orchard.GetContentItemByHandleAsync(
        "alias:main-menu",
        VersionOptions.Published);
}
```

### C# services

Inject `IContentHandleManager` to resolve any registered handle, including an alias, then use `IContentManager` to load the required version:

```csharp
using OrchardCore.ContentManagement;

public sealed class MenuProvider
{
    private readonly IContentHandleManager _contentHandleManager;
    private readonly IContentManager _contentManager;

    public MenuProvider(
        IContentHandleManager contentHandleManager,
        IContentManager contentManager)
    {
        _contentHandleManager = contentHandleManager;
        _contentManager = contentManager;
    }

    public async Task<ContentItem> GetMainMenuAsync()
    {
        var contentItemId = await _contentHandleManager
            .GetContentItemIdAsync("alias:main-menu");

        return contentItemId is null
            ? null
            : await _contentManager.GetAsync(contentItemId, VersionOptions.Published);
    }
}
```

`IContentHandleManager.GetContentItemIdAsync()` returns `null` when no provider resolves the handle.

## Indexes and queries

### YesSql

`AliasPartIndex` is a YesSql map index with these properties:

| Property | Purpose |
| --- | --- |
| `Alias` | Lowercase alias used for matching. |
| `ContentItemId` | Stable ID returned by alias-handle lookup. |
| `Latest` | Indicates that the indexed version is the latest version. |
| `Published` | Indicates that the indexed version is published. |

The content-handle API is preferred for normal lookups because it composes with other `IContentHandleProvider` implementations.
Code that queries `AliasPartIndex` directly must normalize the supplied alias with `ToLowerInvariant()` and explicitly filter `Published` or `Latest` when version status matters.

```csharp
using OrchardCore.Alias.Indexes;
using YesSql;

var normalizedAlias = alias.ToLowerInvariant();
var index = await session.QueryIndex<AliasPartIndex>(x =>
    x.Published && x.Alias == normalizedAlias).FirstOrDefaultAsync();
```

`AliasPart` also contributes its value to Orchard Core's content indexing pipeline as a stored keyword. This allows index profiles that include the part to use the alias without tokenizing it as full text.

### GraphQL

When the GraphQL feature is enabled, a content type containing `AliasPart` exposes the `aliasPart` field and an indexed `where` filter.
GraphQL content queries return published items by default.

For an `Article` content type:

```graphql
query {
  article(where: {
    aliasPart: {
      alias: "support"
    }
  }) {
    contentItemId
    displayText
    aliasPart {
      alias
    }
  }
}
```

String operators such as `alias_in`, `alias_contains`, `alias_starts_with`, and their negated forms are also available inside `aliasPart`.
Use lowercase filter values because the underlying alias index stores the normalized lowercase value.

## Shape alternates

When `OrchardCore.Contents` is enabled, content shapes receive alias-specific alternates:

```text
Content__Alias__main__menu
Content_Summary__Alias__main__menu
```

When `OrchardCore.Widgets` is enabled, widget shapes receive corresponding alternates:

```text
Widget__Alias__main__menu
Widget_Summary__Alias__main__menu
```

Alternate elements are encoded using Orchard Core's shape-alternate rules. For example, the alias `main-menu` becomes `main__menu`.
See [Templates](../Templates/README.md) for Razor and Liquid template filename examples.

## Extending handle lookup

Alias lookup is integrated through `IContentHandleProvider`. Applications can register another provider to support a different handle prefix:

```csharp
using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;

public interface IExternalContentIdLookup
{
    Task<string> GetContentItemIdAsync(string externalId);
}

public sealed class ExternalIdHandleProvider : IContentHandleProvider
{
    private const string Prefix = "external:";
    private readonly IExternalContentIdLookup _lookup;

    public ExternalIdHandleProvider(IExternalContentIdLookup lookup)
    {
        _lookup = lookup;
    }

    public int Order => 200;

    public Task<string> GetContentItemIdAsync(string handle)
    {
        if (!handle.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<string>(null);
        }

        return _lookup.GetContentItemIdAsync(handle[Prefix.Length..]);
    }
}
```

Register the provider in the module startup:

```csharp
services.AddScoped<IContentHandleProvider, ExternalIdHandleProvider>();
```

Providers run in ascending `Order`, and lookup stops when one returns a non-empty content item ID. Use a distinct prefix to avoid ambiguity with `alias:`, `slug:`, and other registered handle formats.

## Permissions

The Alias module does not define a dedicated permission.

- Configuring `AliasPart` on a content type requires **Edit content types**.
- Editing an alias is governed by the permissions for editing that content item.
- Enabling or disabling the feature requires **Manage features**.

## Troubleshooting

### A lookup returns no content

- Confirm that the Alias feature is enabled on the current tenant.
- Confirm that `AliasPart` is attached to the content type and the item has a non-empty alias.
- Use the `alias:` prefix with generic handle APIs.
- Use `Content.Latest[...]` or request `VersionOptions.Latest` when the item exists only as a draft.
- For `Content.Alias[...]`, confirm that the item is published; this convenience accessor queries published rows only.
- Remember that aliases and their indexes are tenant-specific.

### A generated alias does not change

Generation runs only while the alias is empty. With the editable option, clear the field and save the item. With the generated-and-disabled option, clear the alias through an import or application code before updating the item.

### An alias is rejected as already in use

Aliases are tenant-wide rather than scoped to a content type. Check drafts, published versions, and other content types for the same value. Also check aliases that differ only by casing because lookup normalizes aliases to lowercase.

### An alias does not create a route

Attach and configure `AutoroutePart`. `AliasPart` provides logical lookup, indexing, and shape alternates; it does not register an HTTP route.
