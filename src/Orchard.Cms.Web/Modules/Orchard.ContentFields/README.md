# Content Fields (Orchard.ContentFields)

## Purpose

This modules provides common content fields.

## Available Fields

| Name | Properties | Shape Type |
| --- | --- | --- |
| TextField | `Text (string)` | `TextField` |
| BooleanField | `Value (bool)` | `BooleanField` |
| HtmlField | `Html (string)` | `HtmlField` |

## Usage

From a `Content` template, you can reference a field's value like this:

```
var fieldValue = contentItem.Content.Article.MyField.Text;
```

If the content type is `Article` and has a Text Field named `MyField`.

