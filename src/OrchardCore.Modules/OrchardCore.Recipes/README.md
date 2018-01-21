# Recipes (OrchardCore.Recipes)

## Recipe helpers

Recipes can use script helpers like this:

```
{
    "ContentItemId": "[js: uuid()]"
}
```

| Name | Description |
| --- | --- |
| `uuid()` | Generates a unique identifier for a content item |
| `base64(string)` | Decodes the specified string from Base64 encoding. Use https://www.base64-image.de/ to convert your files to base64. |
| `html(string)` | Decodes the specified string from HTML encoding |
| `gzip(string)` | Decodes the specified string from gzip/base64 encoding. Use http://www.txtwizard.net/compression to gzip your strings. |

