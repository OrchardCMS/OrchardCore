# Dynamic Cache (`OrchardCore.DynamicCache`)

## Purpose

Dynamic Cache allows you to cache sections of markup.  
Each cached section of markup can contain other (child) cached sections of markup.

Cached sections can all have their own cache policies, which allows for finer configuration options than a page level cache would have.

Cached values are stored using the `IDynamicCache` service.  
Its default implementation is based on `IDistributedCache` which is itself based on `IMemoryCache`.

### Example

Layout (not cached)

- Section A
  - Section A1 (varies by role)
  - Section A2
- Section B
  - Section B1 (varies by query string)
  - Section B2

## Rendering cached sections

When this page is rendered for the first time, all shapes will be evaluated. Any blocks of markup that have been identified as cacheable will be stored in the `IDynamicCache` service.

On subsequent requests, if a cacheable section has already been cached (and the cache entry is still valid) then it won't be processed (`Processing` event in
the `ShapeMetadata`). The markup will be retrieved from the cache and returned as part of the response.

## Invalidating cached sections
If a cached section is invalidated, the markup for the section will be regenerated on the next request, and then placed back into the cache for subsequent requests to take advantage of.

- If its children are still cached, then their cached value will be used.
- Invalidating a block will also invalidate all parent blocks.

For instance, if `Section B2` is invalidated, `Section B` will also be invalidated. When the Layout is rendered, the `Section B` code will run 
again, as will `Section B2`, but the cached content of `Section B1` will be reused.

Cached sections can define dependencies, which allows the cache to know when the cached value should be invalidated.

For example, if a cache section includes the body of a content item, you may want to automatically invalidate this cache section whenever that content item changes.
You can do this by adding the dependencies `contentitemid:{ContentItemId}` to the cache section. 

Cached sections can also be configured with a sliding expiration window, an absolute expiration window, or both.  
If no expiration window is provided, a default sliding window of one minute will be used.  
If both types of expiration windows are supplied, the sliding policy will be used, up to the maximum absolute time allowed by the absolute expiration window.

### Well-known Cache dependencies

Here is a list of common cache dependency values that can be used to invalidate cache entries.

| Dependency | Description |
| --------- | ----------- |
| `contentitemid:{ContentItemId}` | Invalidated when a content item described with its unique id (`{ContentItemId}`) is Published, Unpublished or Removed. |
| `alias:{Alias}` | Invalidated when a content item with a specific alias (`{Alias}`) is Published, Unpublished or Removed. |

You can create your own dependencies by calling `RemoveTagAsync()` on `ITagCache` in response to events.

## Varying cached sections (Contexts)

You may have a cached section that needs to be varied depending on the context of the request.  
An example of this would be a header section that is included on every page on the site, but contains different markup for each user (e.g. a log in form, or the currently logged in user's username etc...').

You can do this by adding 'vary by' values (called contexts) to the cache policy of a cached section.

Adding a `user` context to the header example given above would create a unique cache item for each user that logs in to your site.

Contexts are hierarchical. For instance if a shape varies by `user` and `user.roles` contexts, only the `user` value will be used
as it's more specialized than the `user.roles` one.

Contexts can be parameterized, for instance `query:age` will pick the `age` value of the query string.

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

You can create your own Contexts by implementing `ICacheContextProvider`.

### Fallback Contexts

Sometimes you may want to vary by a known value that is not an available context.

For example: You may wish to cache all your blog posts so that you can quickly display lists of your posts throughout your site. If the cache ID for the cache block was `blog-post`, you can use a known value as a context to vary the cache item for each blog post. In this case, you could use the Content Item ID as a context:

```liquid
{% cache "blog-post-summary", vary_by: Model.ContentItem.ContentItemId %}
    ...
{% endcache %}
```

## Usage

Cached sections can be configured to encompass a shape, or they can be explicitly added to markup with the `cache` liquid block, or the `cache` razor tag helper:

### Caching a shape

`ShapeMetadata.Cache(string cacheId)`

When called on a shape instance, marks the shape as being cached. Returns a `CacheContext` object.

Example: `myShape.Cache("myshape")`

#### CacheContext members

| Method | Description |
| --------- | ----------- |
| `WithDuration(Timespan)` | Cache the shape for the specified amount of time. |
| `WithSlidingExpiration(Timespan)` | Cache the shape for a specific amount of time with a sliding window. |
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

#### Shape Tag Helper Attributes

When using shape tag helpers, the following attributes can be used:

| Razor Attribute | Liquid Attribute | Description | Required |
| --------- | ----------- | ----------- | ----------- |
| `cache-id` | `cache_id` | The identifier of the cached shape. | Yes |
| `cache-context` | `cache_context` | A set of space/comma-separated context values. | No |
| `cache-dependency` | `cache_dependency` | A set of space/comma-separated dependency values. | No |
| `cache-tag` | `cache_tag` | A set of space/comma-separated tag values. | No |
| `cache-fixed-duration` | `cache_fixed_duration` | The cache duration of the entry, e.g. "00:05:00" for 5 minutes. | No |
| `cache-sliding-duration` | `cache_sliding_duration` | The sliding cache duration of the entry, e.g. "00:05:00" for 5 minutes. | No |

For example, to cache the menu shape in a liquid template, you would use this markup:

`{% shape "menu", alias: "alias:main-menu", cache_id: "main-menu", cache_fixed_duration: "00:05:00", cache_tag: "alias:main-menu" %}`

To cache a content item shape in a liquid template, you could use this markup:

`{% contentitem alias: "alias:main-menu", cache_id: "main-menu", cache_fixed_duration: "00:05:00", cache_tag: "alias:main-menu" %}`

### Liquid cache block

The liquid `cache` block can be used to cache sections of markup. `cache` blocks can be nested.

#### Arguments

| Liquid Attribute | Description | Required |
| --------- | ----------- | ----------- |
| `id` | The identifier of the cached shape. | Yes (this is the default first argument --- no need to explicitly specify the name of this argument.)  |
| `contexts` | A set of space/comma-separated context values. | No |
| `dependencies` | A set of space/comma-separated dependency values. | No |
| `expires_after` | The cache duration of the entry, e.g. "00:05:00" for 5 minutes. | No |
| `expires_sliding` | The sliding cache duration of the entry, e.g. "00:05:00" for 5 minutes. | No |

#### Examples

Simple block:

```liquid
{% cache "my-cache-block" %}
    ...
{% endcache %}
```

Nested blocks:

```liquid
{% cache "a" %}
    A {{ "now" | date: "%T" }} (No Duration) <br />
    {% cache "a1", dependencies: "a1", vary_by: "user", expires_after: "0:5:0" %}
        A1 {{ "now" | date: "%T" }} (5 Minutes) <br />
    {% endcache %}
    {% cache "a2", dependencies: "a2", expires_after: "0:0:1" %}
        A2 {{ "now" | date: "%T" }} (1 Second) <br />
        {% cache "a2a", dependencies: "a2a", contexts: "route", expires_sliding: "0:0:5" %}
            A2A {{ "now" | date: "%T" }} (5 Seconds) <br />
        {% endcache %}
    {% endcache %}
{% endcache %}
```

### Altering a cache scope

You may not yet know all the child dependencies, or even how long the cache block should be cached for when you enter a cache block.  
An example might be a cache block around a list of content items from a query --- because you do not know which content items will be displayed by the query, you cannot define the correct dependencies when you enter the cache block.

There are four tags that allow you to alter the current cache scope. It's safe to use these tags even if you don't necessarily know if you're inside a cache block:

| Liquid Tag | Description | Example |
| --------- | ----------- | ----------- |
| `cache_dependency` | Adds a dependency to the current cache scope. | `{% cache_dependency "alias:{Alias}" %}` |
| `cache_expires_on` | Sets a fixed date and time that the cache item will expire. The most restrictive cache policy (i.e. the one with the shortest life) will win in the event of multiple expiry policies being defined for a single block.  | `{% cache_expires_on {A DateTime or DateTimeOffset instance %}` (e.g. from a date/time field on a content item) |
| `cache_expires_after` | Sets a timespan relative to when the item was cached that the cache item will expire. The most restrictive cache policy (i.e. the one with the shortest life) will win in the event of multiple expiry policies being defined for a single block. | `{% cache_expires_after "01:00:00" %}` (One hour) |
| `cache_expires_sliding` | Sets a sliding window for the expiry of the cache item. The most restrictive cache policy (i.e. the one with the shortest life) will win in the event of multiple expiry policies being defined for a single block. | `{% cache_expires_sliding "00:01:00" %}` (One minute) |

#### Example:

Displaying content items from a query:

```liquid
{% cache "recent-blog-posts" %}
    {% assign recentBlogPosts = Queries.RecentBlogPosts | query %}
    {% for item in recentBlogPosts %}
        {{ item | display_text }}

        {% assign cacheDependency = "contentitemid:" | append: Model.ContentItem.ContentItemId %}
        {% cache_dependency cacheDependency %}
    {% endfor %}
{% endcache %}
```

Each item that is displayed by the query will now add its own cache dependency to the `recent-blog-posts` cache block.

### Razor cache tag helper

!!! note
    This has been renamed from `<cache>` to `<dynamic-cache>` to prevent collisions with the ASP.NET Core cache tag helper.
