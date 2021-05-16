# Layers (`OrchardCore.Layers`)

The layers can be managed from the `Design > Widgets` page in the admin.

A layer has a name, a description and a rule in which you specify a condition to render the widgets that will be associated to this layer.

When you add a widget into a zone, you must also select a Layer to associate the widget with. The widget is displayed if the layer's corresponding display rule evaluates to true.

You can select the checkbox next to each layer in order to highlight the associated Widgets on the left.

In the rule you may specify multiple conditions which must evaluate to true, or you can use condition groups, `All` or `Any` to vary the rule.

!!! note
    Layer rules have been upgraded from a single javascript rule to conditions during RC2, so this document may differ depending on your version.
    A migration converts existing javascript rules into either matching conditions, or javascript conditions.

## Conditions

Here are some available conditions:

| Condition | Description |
| -------- | ----------- |
| `Homepage` | Whether the current page is the site homepage |
| `Is anonymous` | Whether the current user is anonymous, i.e. not authenticated. |
| `Is authenticated` | Whether the current user is authenticated. |
| `Role` | A role condition evaluates the current users roles against a value. |
| `Url` | A url condition evaluates the current url against a value. |
| `Culture` | A culture condition evaluates the current ui culture against a value. |
| `Content Type` | A content type condition evaluates the currently displayed content types against a value. |
| `Javascript` | A script condition written in javascript. |
| `All` | An all condition group contains other conditions which are all required to be true. |
| `Any` | An any condition group contains other conditions but only requires any condition to be true. |
| `Boolean` | A boolean condition evaluating to `true` or `false`. |

Ex: The `Always` layer has a `Boolean Condition` set to `true`, so widgets on this layer will always be shown.

Refer [Rules](../Rules/README.md) for more information about creating custom conditions.

Refer [Scripting](../Scripting/README.md#layers-orchardcorelayers) for more information about the available javascript methods.

## Zones

The zones that are listed can be set in the `Design > Settings > Zones` admin page.

You must have declared the corresponding zones as sections in your theme :

=== "Liquid"

    ``` liquid
    {% render_section "Header", required: false %}
    ```

=== "Razor"

    ``` html
    @await RenderSectionAsync("Header", required: false)
    ```
