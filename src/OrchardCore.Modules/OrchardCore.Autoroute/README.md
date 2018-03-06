# Autoroute (OrchardCore.Autoroute)

This module allows you to specify custom URLs for your content items.

## Autoroute Part

Attach this part to a content type to specify custom URLs to your content items.

## Autoroute Alias

Content items with an Autoroute can be retrieved by URL anywhere you can retrieve content by alias (see example below). The syntax for this is `slug:<URL>`, e.g. `slug:my-blog/my-blog-post`.

## Liquid

With Autoroute enabled, you can retrieve content by URL in your liquid views and templates:

```
{% assign my_content = Content["slug:my-blog/my-blog-post"] %}
```

or

```
{% assign my_content = Content.Slug["my-blog/my-blog-post"] %}
```

