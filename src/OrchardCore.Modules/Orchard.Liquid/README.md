# Liquid (Orchard.Liquid)

This module provides a way to create templates securely from the admin site.
For more information about the Liquid syntax, please refer to this site: https://shopify.github.io/liquid/

## General concepts

### HTML escaping

All outputs are encoded into HTML by default. It means that any string that returns some HTML reserved chars will
be converted to the corresponding HTML entities. If you need to render some raw HTML chars you can use the `Raw` filter.

## Filters

All the default filters that are available in the standard Liquid syntax are available in Orchard. On top of that each Orchard module can
provide custom filters for their own purpose. Here is a list of common filters that apply to content items.

### display_url

Returns the url of the content item

Input
```
{{ ContentItem | display_url }}
```

Output
```
/blog/my-blog-post
```

### display_text

Returns the title of the content item

Input
```
{{ ContentItem | display_text }}
```

Output
```
My Blog Post
```

### slugify

Convert a text into a string that can be used in a url.

Input
```
{{ "This is some text" | slugify }}
```

Output
```
this-is-some-text
```

### container

Returns the container content item of another content item.


Input
```
{{ ContentItem | container | display_text }}
```
In this example we assume `ContentItem` represents a blog post.

Output
```
Blog
```

### local

Converts a UTC date and time to the local date and time based on the site settings.

Input
```
{{ "now" | local | date: "%c" }}
or
{{ ContentItem.CreatedUtc | local | date: "%c" }}
```

Output
```
Wednesday, 02 August 2017 11:54:48
```

## Properties

By default the liquid templates have access to a common set of objects.

### ContentItem

Represents the current content item that contains the liquid template.

The following properties are available on the `ContentItem` object.

| Property | Example | Description |
| --------- | ---- |------------ |
| `Id` | `12` | The id of the document in the database |
| `ContentItemId` | `4qs7mv9xc4ttg5ktm61qj9dy5d` | The common identifier of all versions of the content item |
| `ContentItemVersionId` | `4jp895achc3hj1qy7xq8f10nmv` | The unique identifier of the content item version |
| `Number` | `6` | The version number |
| `Owner` | `admin` | The username of the creator of this content item |
| `Author` | `admin` | The username of the editor of this version |
| `Published` | `true` | Whether this content item version is published or not |
| `Latest` | `true` | Whether this content item version is the latest of the content item |
| `ContentType` | `BlogPost` | The content type |
| `CreatedUtc` | `2017-05-25 00:27:22.647` | When the content item was first created or first published |
| `ModifiedUtc` | `2017-05-25 00:27:22.647` | When the content item version was created |
| `PublishedUtc` | `2017-05-25 00:27:22.647` | When the content item was last published |
| `Content` | `{ ... }` | A document containing all the content properties. See specific documentation for usage .|

#### Content property

The `Content` property of a content item exposes all its elements, like parts and fields. It is possible to
inspect all the available properties by evaluating `Content` directly. It will then render the full document.

The convention is that each Part is exposed by its name as the first level.
If the content item has custom fields, they will be available under a part whose name will match the content type.

For example, assuming the type `Product` has a Text field named `Size`, access the value of this field for a 
content item would be:

```
{{ ContentItem.Content.Product.Size.Text }}
```

Similarly, if the content item has a `Title` part, we can access it like this:

```
{{ ContentItem.Content.TitlePart.Title }}
```

### User

Represents the authenticated user for the current request.

The following properties are available on the `User` object.

| Property | Example | Description |
| --------- | ---- |------------ |
| `Identity.Name` | `admin` | The name of the authenticated user |

### Queries

The `Queries` object provide a way to access predefined queries from the Queries module.

To access a named query, use the name as a property on the `Queries` object like this:

```
{% for item in Queries.MyQuery %}
{{ item | display_text }}
{% endfor %}
```

The example above will iterate over all the results of the query name `MyQuery` and display the text representing
the content item. Any available property on the results of the queries can be used. This example assumes the results
will be content items. Refer to the Queries module documentation on how to create custom queries.
