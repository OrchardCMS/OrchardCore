# Recipes (Orchard.Recipes)

## Recipe helpers

Recipes can use script helpers like this:

```
{
    ContentItemId": "[js: uuid()]"
}
```

| Name | Description |
| --- | --- |
| `uuid()` | Generates a unique identifier for a content item |
| `base64(string)` | Decodes the specified string from Base64 encoding |
| `html(string)` | Decodes the specified string from HTML encoding |

