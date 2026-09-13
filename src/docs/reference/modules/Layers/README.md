# Layers (`OrchardCore.Layers`)

The Layers can be managed from the `Design > Widgets` page in the admin.

A Layer has a name, a description and a rule in which you specify a condition to render the widgets that will be associated to this Layer.

For remote layer definitions, condition discovery and validation, see the
[Layers API](../../api/layers/README.md). It supplies Pomi commands and eligible MCP tools
using the same tenant document and `ManageLayers` permission.

![Create Layer.](./assets/create-layer.png)

_The Layer needs to be saved first to reveal the Rule option._

When you add a widget into a zone, you must also select a Layer to associate the widget with. The widget is displayed if the Layer's corresponding display rule evaluates to true.

![Add widget.](./assets/add-widget.png)

You can select the checkbox next to each Layer in order to highlight the associated Widgets on the left.

In the rule you may specify multiple conditions which must evaluate to true, or you can use condition groups, `All` or `Any`, to vary the rule.

![Rules](./assets/rules.png)

_There can be multiple Rules in place for one Layer._

!!! note
    Layer rules have been upgraded from a single JavaScript rule to conditions during RC2, so this document may differ depending on your version.
    A migration converts existing JavaScript rules into either matching conditions or JavaScript conditions.

## Conditions

Here are some available conditions:

| Condition          | Description                                                                                  |
|--------------------|----------------------------------------------------------------------------------------------|
| `Homepage`         | Whether the current page is the site homepage                                                |
| `Is anonymous`     | Whether the current user is anonymous, i.e. not authenticated.                               |
| `Is authenticated` | Whether the current user is authenticated.                                                   |
| `Role`             | A role condition evaluates the current users roles against a value.                          |
| `Url`              | A url condition evaluates the current url against a value.                                   |
| `Culture`          | A culture condition evaluates the current ui culture against a value.                        |
| `Content Type`     | A content type condition evaluates the currently displayed content types against a value.    |
| `Javascript`       | A script condition written in JavaScript.                                                    |
| `All`              | An all condition group contains other conditions which are all required to be true.          |
| `Any`              | An any condition group contains other conditions but only requires any condition to be true. |
| `Boolean`          | A boolean condition evaluating to `true` or `false`.                                         |

Ex: The `Always` Layer has a `Boolean Condition` set to `true`, so widgets on this Layer will always be shown.

Refer to [Rules](../Rules/README.md) for more information about creating custom conditions.

Refer to [Scripting](../Scripting/README.md#layers-orchardcorelayers) for more information about the available JavaScript methods.

## Zones

The zones that are listed can be set in the `Design > Settings > Zones` admin page.

You must have declared the corresponding zones as sections in your theme:

=== "Liquid"

    ``` liquid
    {% render_section "Header", required: false %}
    ```

=== "Razor"

    ``` html
    @await RenderSectionAsync("Header", required: false)
    ```

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/NCvytsdED_o" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Recipe Configuration

Layer settings can be configured using the `Settings` recipe step:

```json
{
  "steps": [
    {
      "name": "settings",
      "LayerSettings": {
        "Zones": [
          "Content",
          "Footer",
          "Header"
        ]
      }
    }
  ]
}
```

| Property | Type            | Description                                       |
|----------|-----------------|---------------------------------------------------|
| `Zones`  | Array of String | The list of available zones for widget placement. |

### Remote widget placement

The [widget placement API](../../api/layers/README.md#widget-placement) exposes
configured zones and authorized widget placements through OpenAPI, Pomi and MCP.
The admin editor and drag-and-drop action share its validation and persistence
service. Moving a published widget requires content edit and publish permission;
draft body changes remain unpublished.

Available zones can also be edited through the
[`layer-zones` settings section](../../api/settings/README.md#layer-zones-section).
The admin editor and remote settings share normalization and mutation logic.
Existing widgets retain their placements when the available-zone list changes.

## Recipe validation and shared services

The `Layers` recipe validates all incoming names and conditions before changing the
layer document. Name validation and named mutations use `ILayerService`; JavaScript
required-value and syntax checks use `IRuleManagementService`, as the management API
and editor do. Validation parses scripts without executing them.

Recipes retain their legacy condition format and support registered extension
condition factories. They are not restricted to the public API's writable condition
schemas. An omitted rule preserves the existing rule; a supplied condition list
replaces its children. Omitted or empty descriptions preserve the existing value.
Existing identities are preserved, and missing root identities are generated by the
layer service. Unknown condition types or invalid definitions reject the prepared
step before any layer mutation.
