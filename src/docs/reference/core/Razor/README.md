# Razor Helpers

Many extensions methods are available in Razor with `@Orchard`.

## Razor extensions

| Method | Module | Description |
| ------ | ------ | ----------- |
| `DisplayAsync(ContentItem content, string displayType = "")` | OrchardCore.ContentManagement.Display | Renders a content item with the corresponding display type. |
| `GetContentCultureAsync(ContentItem contentItem)` | OrchardCore.ContentLocalization | Returns the culture for a given ContentItem. |
| `CultureDir()` | OrchardCore.DisplayManagement | Returns the current culture direction. |
| `CultureName()` | OrchardCore.DisplayManagement | Returns the current culture name. |
| `ResourceUrl(string resourcePath, bool? appendVersion = null)` | OrchardCore.ResourceManagement | Prefixes the Cdn Base URL to the specified resource path. |
| `GetContentItemIdByAliasAsync(string alias)` | OrchardCore.Alias | Returns a content item id from its alias. Ex: `carousel` |
| `GetContentItemIdBySlugAsync(string slug)` | OrchardCore.Autoroute | Returns a content item id from its slug. Ex: `myblog/my-blog-post` |
| `GetContentItemIdByHandleAsync(string handle)` | OrchardCore.Contents | Returns a content item id from its handle. Ex: `alias:carousel`, `slug:myblog/my-blog-post` |
| `GetContentItemByAliasAsync(string alias, bool latest = false)` | OrchardCore.Alias | Loads a content item by its alias, seeking the latest version or not. Ex: `carousel` |
| `GetContentItemBySlugAsync(string slug, bool latest = false)` | OrchardCore.Autoroute | Loads a content item by its slug, seeking the latest version or not. Ex: `slug:myblog/my-blog-post`|
| `GetContentItemByHandleAsync(string handle, bool latest = false)` | OrchardCore.Contents | Loads a content item by its handle, seeking the latest version or not. Ex: `alias:carousel`, `slug:myblog/my-blog-post`|
| `GetContentItemByIdAsync(string contentItemId, bool latest = false)` | OrchardCore.Contents | Loads a content item by its id. |
| `GetContentItemsByIdAsync(IEnumerable<string> contentItemIds, bool latest = false)` | OrchardCore.Contents | Loads a list of content items by their ids. |
| `GetContentItemByVersionIdAsync(string contentItemVersionId)` | OrchardCore.Contents | Loads a content item by its version id. |
| `QueryContentItemsAsync(Func<IQuery<ContentItem, ContentItemIndex>, IQuery<ContentItem>> query)` | OrchardCore.Contents | Query content items. |
| `GetRecentContentItemsByContentTypeAsync(string contentType, int maxContentItems = 10)` | OrchardCore.Contents | Loads content items of a specific type. |
| `LiquidToHtmlAsync(string liquid)` | [OrchardCore.Liquid](../../modules/Liquid/README.md#razor-helpers) | Parses a liquid string to HTML. |
| `LiquidToHtmlAsync(string liquid, object model)` | [OrchardCore.Liquid](../../modules/Liquid/README.md#razor-helpers) | Parses a liquid string to HTML. |
| `SanitizeHtml(string html)` | [OrchardCore.Infrastructure](../Sanitizer/README.md#razor-helper) | Sanitizes an HTML string. |
| `QueryListItemsCountAsync(string listContentItemId, Expression<Func<ContentItemIndex, bool>> itemPredicate = null)` | OrchardCore.Lists | Returns list count. |
| `QueryListItemsAsync(string listContentItemId, Expression<Func<ContentItemIndex, bool>> itemPredicate = null)` | [OrchardCore.List](../../modules/Lists/README.md#orchard-helpers) | Returns list items. |
| `MarkdownToHtmlAsync(string listContentItemId, Expression<Func<ContentItemIndex, bool>> itemPredicate = null)` | [OrchardCore.Markdown](../../modules/Markdown/README.md#razor-helper) | Converts Markdown string to HTML. |
| `AssetUrl(string assetPath, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined, bool appendVersion = false)` | [OrchardCore.Media](../../modules/Media/README.md#razor-helpers) | Returns the relative URL of the specifier asset path with optional resizing parameters. |
| `ImageResizeUrl(string imagePath, int? width = null, int? height = null, ResizeMode resizeMode = ResizeMode.Undefined)` | [OrchardCore.Media](../../modules/Media/README.md#razor-helpers) | Returns a URL with custom resizing parameters for an existing image path. |
| `ContentQueryAsync(string queryName)` | [OrchardCore.Queries](../../modules/Queries/README.md#razor-helpers) | Returns a List of Content items |
| `ContentQueryAsync(string queryName, IDictionary<string, object> parameters)` | [OrchardCore.Queries](../../modules/Queries/README.md#razor-helpers) | Returns a List of Content items |
| `QueryAsync(string liquid, object model)` | [OrchardCore.Queries](../../modules/Queries/README.md#razor-helpers) | Returns a List of objects |
| `QueryAsync(string queryName, IDictionary<string, object> parameters)` | [OrchardCore.Queries](../../modules/Queries/README.md#razor-helpers) | Returns a List of objects |
| `ShortcodesToHtmlAsync(string html, object model = null)` | [OrchardCore.Shortcodes](../../modules/Shortcodes/README.md#rendering-shortcodes) | Renders shortcodes. |
| `GetTaxonomyTermAsync(string taxonomyContentItemId, string termContentItemId)` | [OrchardCore.Taxonomies](../../modules/Taxonomies/README.md#orchard-helpers) | Returns a the term from its content item id and taxonomy. |
| `GetInheritedTermsAsync(string taxonomyContentItemId, string termContentItemId)` | [OrchardCore.Taxonomies](../../modules/Taxonomies/README.md#orchard-helpers) | Returns the list of terms including their parents. |
| `QueryCategorizedContentItemsAsync(string taxonomy(Func<IQuery<ContentItem, TaxonomyIndex>, IQuery<ContentItem>> query)` | [OrchardCore.Taxonomies](../../modules/Taxonomies/README.md#orchard-helpers) | Query content items. |

## How to use

If you want to use an extension method in a view, you can inject an `IOrchardHelper` named `Orchard` at the top of your file:

```csharp
@inject OrchardCore.IOrchardHelper Orchard
```

In `OrchardCore.DisplayManagement.Razor`, there is a RazorPage that already has a public property `Orchard` that you can use to call an extension method or the current `HttpContext`.

If you want to use an Orchard helper in a controller, you can inject an instance in the constructor:

```csharp
private IOrchardHelper _orchard;

public MyClass(IOrchardHelper orchard)
{
	_orchard = orchard;
}
```

!!! note
    If the extension method you want to use cannot be found (in a Theme for example), do not forget to reference the corresponding module.
