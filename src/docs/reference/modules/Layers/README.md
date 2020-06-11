# Layers (`OrchardCore.Layers`)

The layers can be managed from the `Design > Widgets` page in the admin.

A layer has a name, a description and a rule in which you specify a condition to render the widgets that will be associated to this layer.

When you add a widget into a zone, you can select the associated Layer for which the widget will be displayed if the corresponding display rule is valid.

You can select the checkbox next to each layer in order to highlight the associated Widgets on the left.

In the rule, you can use a javascript function that will return a boolean.  
Ex: The `Always` rule has the rule set to `true`.

## Rules

Here are some available functions:

| Function | Description |
| -------- | ----------- |
| `isHomepage(): Boolean` | Returns true if the current request Url is the current homepage |
| `isAnonymous(): Boolean` | Returns true if there is no authenticated user on the current request |
| `isAuthenticated(): Boolean` | Returns true if there is an authenticated user on the current request |
| `url(url: String): Boolean` | Returns true if the current url matches the provided url. Add a `*` to the end of the url parameter to match any url that start with  |
| `culture(name: String): Boolean` | Returns true if the current culture name or the current culture's parent name matches the `name` argument |

You can add new functions by implementing an `IGlobalMethodProvider`. See [Scripting](../Scripting/README.md#layers-orchardcorelayers)

## Zones

The zones that are listed can be set in the `Design > Settings > Zones` admin page.

You must have declared the corresponding zones as sections in your theme :

```liquid tab="Liquid"
{% render_section "Header", required: false %}
```

``` html tab="Razor"
@await RenderSectionAsync("Header", required: false)
```
