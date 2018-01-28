# Contents (OrchardCore.Contents)

This module provides Content Management services.

## Liquid Filters

### content

Returns a content item.

#### Loading from a content item id

Loads a content item from its unique identifier. 

```
{% assign my_content = "417qsjrgv97e74wvp149h4da53" | content %}
```

By default it loads the _published_ version. The _latest_ version (either "published" or "draft") can be loaded using the `latest: true` option.

```
{% assign my_content = "417qsjrgv97e74wvp149h4da53" | content: latest: true %}
```

#### Loading from an alias

```
{% assign my_content = "alias:main-menu" | content: 'alias' %}
```

The latest version can be loaded using the `latest: true` option. 

Aliases can be in various forms, like when using Autoroute, with the `slug` prefix.

```
{% assign my_content = "slug:my-blog/my-blog-post" | content: 'alias' %}
```

> Aliases are provided by implementing `IContentAliasProvider`.

#### Loading from a content version id

```
{% assign my_content = "417qsjrgv97e74wvp149h4da53" | content: 'version' %}
```

## Razor Helper

The following methods are available from the Razor helper in the `OrchardCore.ContentManagement` namespace.

| Method | Parameters | Description |
| --------- | ---- |------------ |
| `GetContentItemIdByAliasAsync` | `string alias` | Returns the content item id from its alias. |
| `GetContentItemByAliasAsync` | `string alias, bool latest = false` | Loads a content item from its alias, seeking the latest version or not. |
| `GetContentItemByIdAsync` | `string contentItemId, bool latest = false` | Loads a content item from its id, seeking the latest version or not. |
| `GetContentItemByVersionIdAsync` | `string contentItemVersionId` | Loads a content item from its version id. |
