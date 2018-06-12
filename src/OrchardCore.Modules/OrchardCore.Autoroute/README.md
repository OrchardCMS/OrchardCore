# Autoroute (OrchardCore.Autoroute)

This module allows you to specify custom URLs (permalink) for your content items.

## Autoroute Part

Attach this part to a content type to specify custom URLs to your content items.

Then, go to the definition of a Content Type and edit the Autoroute Part:

- Enter a Pattern using a Liquid expression that will represent the generated slug.

Example for a content with a TitlePart that will use it to generate the slug :

```liquid
{{ ContentItem | display_text | slugify }}
```

Example for a content with a ListPart and a TitlePart (ig: BlogPost nested in a Blog) that will use the container and the title to generate the slug:

```liquid
{{ ContentItem | container | display_text | slugify }}/{{ ContentItem | display_text | slugify }}`
```

- If you want to be able to enter a custom path when you edit a content item, check 'Allow custom path'.
- If you want to be able to set a content item as the homepage, check 'Show homepage options'

## Autoroute Alias

Content items with an Autoroute can be retrieved by URL anywhere you can retrieve content by alias (see example below). The syntax for this is `slug:<URL>`, e.g. `slug:my-blog/my-blog-post`.

## Liquid

With Autoroute enabled, you can retrieve content by URL in your liquid views and templates:

```liquid
{% assign my_content = Content["slug:my-blog/my-blog-post"] %}
```

or

```liquid
{% assign my_content = Content.Slug["my-blog/my-blog-post"] %}
```

