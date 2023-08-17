# Autoroute (`OrchardCore.Autoroute`)

This module allows you to specify custom URLs (permalinks) for your content items.

## Autoroute Part

Attach this part to a content type to specify custom URLs to your content items.

Then, go to the definition of a Content Type and edit the Autoroute Part:

- Enter a Pattern using a Liquid expression that will represent the generated slug.

For example, for a content with a `TitlePart` that will use it to generate the slug :

```liquid
{{ ContentItem | display_text | slugify }}
```

For instance, for a content type with a `ListPart` and a `TitlePart` (e.g.: `BlogPost` nested in a `Blog`) that will use the container and the title to generate the slug:

```liquid
{{ ContentItem | container | display_text | slugify }}/{{ ContentItem | display_text | slugify }}
```

- If you want to be able to enter a custom path when you edit a content item, check 'Allow custom path'.
- If you want to be able to set a content item as the homepage, check 'Show homepage options'.

### Using fields in patterns

Fields can also be used as part of the pattern. The following example uses a __Text field__ named `Color`, on a `Product` content type. The text is _slugified_ such that
it is compatible with a URL.

```liquid
{{ ContentItem.Content.Product.Color.Text | slugify }}
```

## Autoroute Slug Handle

Content items with an `Autoroute` can be retrieved by URL anywhere you can retrieve content by its slug handle (see example below). The syntax for this is `slug:<URL>`, e.g. `slug:my-blog/my-blog-post`.

## Liquid

With `Autoroute` enabled, you can retrieve content by URL in your liquid views and templates:

```liquid
{% assign my_content = Content["slug:my-blog/my-blog-post"] %}
```

or

```liquid
{% assign my_content = Content.Slug["my-blog/my-blog-post"] %}
```

## Container Routing

The `AutoroutePart` supports routing of content items which are children of a parent content item.

### Container and Contained Definitions

### Container Definition

A _container_ content item is a parent content item, which _contains_ child content items.

For example :

- A content item with a `BagPart` attached is a _container or parent_ content item.

- A `Taxonomy` is also a _container_ content item.

### Contained Definition

A _contained_ content item refers to a content item which is _contained_ inside a _container_ content item.

For example :

- Content items _contained_ inside a `BagPart` are considered _contained or child_ content items.

- `Terms` of a `Taxonomy` are _contained_ by the `Taxonomy`.

_Contained_ content items are stored as part of the json inside the _container_ document.

### Supported Containers

The `AutoroutePart` supports routing of these _container_ types.

- `BagPart` and content items _contained_ by the `BagPart`
- `Taxonomy` content items and _contained_ `Terms`

### Configuration

To enable routing of _contained_ content items the `AutoroutePart` must be configured correctly.

- Add the `AutoroutePart` to the _container or parent_ content type definition.
- Enable `Allow contained item routing` on the `AutoroutePart Settings`.
- Enable `Route Contained Items` on the _container_ content item.

Optionally, add the `AutoroutePart` to the content type definition of the _contained or child_ content items, to manage the contained item routes.

#### Path Generation

By default when the `AutoroutePart` is added to a _container_ content item and `Route Contained Items` is enabled the generated route will be made up of the _container_ segment, then the `ContentItemId` of the _contained_ content item, and if present, the `DisplayText`.

For example :
`https://www.mysite.com/categories/47twnxzx9hs5k3dyn9j1mc5rny-travel`

To configure a friendly slug for the child content items add the `AutoroutePart` to the content type definition for those content types.

You are then able to use the `Liquid` pattern to generate a friendly slug.

For example :
`https://www.mysite.com/categories/travel`

Routing is, by default, relative to the parent, and there is no `Liquid` filter for the parent as there is with the `ListPart`

#### AutoroutePart Settings

Settings on the `AutoroutePart` allow the site administrator to control how _container and contained_ routing is enabled for a user.

##### Allow Route Contained Items

Enable this on a `container` content type definition, i.e. the parent, to allow a user to turn on routing for _contained_, i.e. child, content items.

##### Manage Contained Item Routes

Enable this on a `contained` content type definition, i.e. the child, to allow the `AutoroutePart` to configure individual control of routing for these content items.

##### Allow Absolute Path

Enabled `AllowAbsolutePath` to allow a user to set a path as absolute.

By default _container_ routing will build a url relative to the _containers_ route.

##### Allow Disabled

Enable this option to allow content editors to disable route generation.

Use this option when you have a _container_ with two `BagParts` and routing should only be enabled for one.

#### AutoroutePart Content Item Editor

##### Disabled

Content editors can select to disabled route generation for a particular _contained_ content item.

##### Route Contained Items

When editing the content item select `Route Contained Items` on the _container_ content item to turn on routing for the _contained_ content items.

##### Absolute

When selected forces the route from relative to absolute.

!!! note
    When you configure _container routing_ for a `BagPart` you will want to change the default display type of the part to `Summary` and create `Summary` and `Detail` templates, for use when the item is displayed within its _container_ and when it is displayed in detail via a route.



