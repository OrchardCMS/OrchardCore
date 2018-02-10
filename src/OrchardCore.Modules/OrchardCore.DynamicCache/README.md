# Dynamic Cache (OrchardCore.DynamicCache)

## Purpose

Dynamic Cache extends Shape events to provide caching capabilities at the shapes level.
Cached shapes can be composed of other cached shapes.

Cached shapes are stored using the `IDynamicCache` service.
Its default implementation is based on `IDistributedCache` which is itself based on `IMemoryCache`.

### Example:

Layout Shape (not cached)

- Shape A
    - Shape A1 (varies by role)
    - Shape A2
- Shape B
    - Shape B1 (varies by query string)
    - Shape B2

## Rendering cached shapes
When this page is rendered for the first time, all shapes will be evaluated and stored in the `IDynamicCache` service.
The content of the cached parent shapes will replace their child ones by placeholders similar to ESI 
(<https://en.wikipedia.org/wiki/Edge_Side_Includes>). 

On subsequent requests, if a shape has been cached then it won't be processed (`Processing` event in
the `ShapeMetadata`). Instead, its ESI tags will be processed and replaced by the child content if it's still valid.

## Invalidating cached shapes
If a shape cache content is invalidated, it will be reprocessed. 

- If its children are still cached then their cached value will be used.
- Invalidating a shape will also invalidate all parent shapes.

For instance, if `Shape B2` is invalidated, `Shape B` will also be invalidated. When the Layout is rendered, the `Shape B` code will run 
again, as will `Shape B2`, but the cached content of `Shape B1` will be reused.

### Well-known Cache tags

Here is a list of common cache tag values that can be used to invalidate cache entries.

| Tag | Description |
| --------- | ----------- |
| `contentitemid:{ContentItemId}` | Triggered when a content item described with its unique id (`{ContentItemId}`) is Published, Unpublished or Removed. |
| `alias:{Alias}` | Triggered when a content item with a specific alias (`{Alias}`) is Published, Unpublished or Removed. |

## Varying contexts
When a shape is cached, it will have a different result for each value of the contexts it defined.
For instance `Shape A1` varies by role, so each page that is rendered for a user of a different role will get a different value for this shape.
The `Shape A` cached content will remain the same and will be reused across requests targeting different user roles.

Contexts are hierarchical. For instance if a shape varies by `user` and `user.roles` contexts, only the `user` value will be used
as it's more specialized than the `user.roles` one.

Contexts can be parameterized, for instance `query:age` will pick the `age` value of the query string.

## Usage

### Caching a shape

`ShapeMetadata.Cache(string cacheId)`

When called on a shape instances, marks the shape as being cached. Returns a `CacheContext` object.

Example: `myShape.Cache("myshape")`

### CacheContext members

| Method | Description |
| --------- | ----------- |
| `During(Timespan)` | Cached the shape for the specific amount of time. |
| `AddContext(params string[])` | Varies the cached content on the specified context values. |
| `RemoveContext(string)` | Removes the specified context. |
| `AddDependency(params string[])` | Defines the context values that will invalidate the cache entry. |
| `RemoveDependency(string)` | Removes the specified dependency. |
| `AddTag(string)` | Adds a tag to the cache entry to that it can be invalidated by this tag value. |
| `RemoveTag(string)` | Removes the specified tag. |

!!! note
    `AddDependency` differs from `AddContext` in that it doesn't store multiple values for each context,
    but invalidates the cached shape content when the value of the context varies.
    Internally they share the same implementation, as the physical cache key will contain the dependency context value.

### Available Contexts

| Context | Description |
| --------- | ----------- |
| `features` | The list of enabled features. |
| `features:{featureName}` | The specified feature name. |
| `query` | The list of querystring values. |
| `query:{queryName}` | The specified query name value. |
| `user` | The current user. |
| `user.roles` | The roles of the current user. |
| `route` | The current request path. |

### Shape Tag Helper Attributes

When using shape tag helpers, the following attributes can be used:

| Context | Description |
| --------- | ----------- |
| `cache-id` | The identifier of the cached shape. |
| `cache-context` | A set of space/comma-separated context values. |
| `cache-dependency` | A set of space/comma-separated dependency values. |
| `cache-tag` | A set of space/comma-separated tag values. |
| `cache-duration` | The cache duration of the entry, e.g. "00:05:00" for 5 minutes. |
