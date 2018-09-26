# Contents (OrchardCore.Contents)

This module provides Content Management services.

## Liquid

You can access content items from liquid views and templates by using the `Content` property. 
By default, you can retrieve content by alias or content item ID. Other modules (such as Alias and Autoroute) allow you to retrieve content by other identifiers.

#### Loading from an alias

```
{% assign my_content = Content["alias:main-menu"] %}
```

Aliases can be in various forms, like when using Autoroute, with the `slug` prefix.

```
{% assign my_content = Content["slug:my-blog/my-blog-post"] %}
```

> Aliases are provided by implementing `IContentAliasProvider`.

#### Loading the latest version of a content item

You can use the `Latest` property to retrieve the latest version of a content item (whether that's the published version or the latest draft version) by alias:

```
{% assign my_content = Content.Latest["alias:main-menu"] %}
```

#### Loading from a content item id

```
{% assign my_content = Content.ContentItemId["417qsjrgv97e74wvp149h4da53"] %}
```

When a list of content item ids is available, the `content_item_id` filter should be preferred:

```
{% assign posts = postIds | content_item_id %}
```

#### Loading from a content item version id

```
{% assign my_content = Content.ContentItemVersionId["49gq8g6zndfc736x0az3zsp4w3"] %}
```

## Razor Helper

The following methods are available from the Razor helper.

| Method | Parameters | Description |
| --------- | ---- |------------ |
| `GetContentItemIdByAliasAsync` | `string alias` | Returns the content item id from its alias. |
| `GetContentItemByAliasAsync` | `string alias, bool latest = false` | Loads a content item from its alias, seeking the latest version or not. |
| `GetContentItemByIdAsync` | `string contentItemId, bool latest = false` | Loads a content item from its id, seeking the latest version or not. |
| `GetContentItemsByIdAsync` | `IEnumerable<string> contentItemIds, bool latest = false` | Loads a list of content items by ids, seeking the latest version or not. |
| `GetContentItemByVersionIdAsync` | `string contentItemVersionId` | Loads a content item from its version id. |


> The Razor Helper is accessible on the `Orchard` property if the view is using Orchard Core's Razor base class, or by injecting `OrchardCore.IOrchardHelper` in all other cases.