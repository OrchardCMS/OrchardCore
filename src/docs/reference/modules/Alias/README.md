# Alias (`OrchardCore.Alias`)

This module allows you to specify friendly identifiers for your content items. Aliases can also be imported and exported, which means that they are persisted when running recipes or deploying content (whereas content item IDs are not).

## Alias Part

Attach this part to a content type to specify aliases for your content items.

## Liquid

With Alias enabled, you can retrieve content by its alias handle in your liquid views and templates:

```liquid
{% assign my_content = Content["alias:footer-widget"] %}
```

or

```liquid
{% assign my_content = Content.Alias["footer-widget"] %}
```
