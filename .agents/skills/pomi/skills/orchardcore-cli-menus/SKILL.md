---
name: orchardcore-cli-menus
description: Creates, updates, publishes, and renders Orchard Core menus through `pomi` using the built-in Menu content model and official Menu/MenuItem/MenuItemLink shapes. Use for menu aliases, link/content/HTML menu items, nested navigation, Liquid menu rendering, caching, active links, and custom menu shape templates.
---

# Pomi CLI Menus

Before issuing commands, read the [shared context, authentication, and output rules](../orchardcore-cli/references/shared-rules.md).
For first-time access or login problems, follow [authentication and contexts](../orchardcore-cli/references/authentication.md).
These rules apply even when this specialist is selected directly.

Manage menus through the content-definition, content-item, and template
commands. Orchard Core currently exposes no separate `pomi menus` CRUD group.
Always use the built-in `Menu` and `MenuItem` stereotypes supplied by
`OrchardCore.Menu`.

Keep navigation separate from page composition. Use Menu content and official
menu shapes for global/local navigation; use Flow section widgets and named
Bags for page sections. Never emulate navigation with Flow widgets or embed
layout/navigation markup in an HTML field.

## Prepare and inspect

```bash
pomi features enable OrchardCore.Menu
pomi api refresh --force
pomi content types show Menu
pomi content types show LinkMenuItem
pomi content types show ContentMenuItem
pomi content types show HtmlMenuItem
pomi content items schema Menu
pomi content items schema LinkMenuItem
pomi content items schema ContentMenuItem
```

Do not recreate these built-in definitions. The Menu feature provides:

- `Menu` with `TitlePart`, `AliasPart`, `MenuPart`, and
  `MenuItemsListPart`;
- `LinkMenuItem`, `ContentMenuItem`, and `HtmlMenuItem` with stereotype
  `MenuItem`;
- nested children through `MenuItemsListPart.MenuItems`.

## Create a menu

Build the complete payload from the live schemas:

```json
{
  "ContentType": "Menu",
  "DisplayText": "Main Menu",
  "TitlePart": {
    "Title": "Main Menu"
  },
  "AliasPart": {
    "Alias": "main-menu"
  },
  "MenuItemsListPart": {
    "MenuItems": [
      {
        "ContentItemId": "5f124f67-7424-4c18-a92a-8aa3d7759cab",
        "ContentType": "LinkMenuItem",
        "DisplayText": "Home",
        "LinkMenuItemPart": {
          "Url": "~/",
          "Target": ""
        }
      },
      {
        "ContentItemId": "426aeaf4-7601-45b4-a9fb-b7e16db8ea0f",
        "ContentType": "LinkMenuItem",
        "DisplayText": "Products",
        "LinkMenuItemPart": {
          "Url": "~/products",
          "Target": ""
        },
        "MenuItemsListPart": {
          "MenuItems": [
            {
              "ContentItemId": "8c4e276e-1a70-4b9f-84bf-98adba07b57e",
              "ContentType": "LinkMenuItem",
              "DisplayText": "Cloud",
              "LinkMenuItemPart": {
                "Url": "~/products/cloud",
                "Target": ""
              }
            }
          ]
        }
      }
    ]
  }
}
```

```bash
pomi content items validate --body-file main-menu.json
pomi content items create-draft --body-file main-menu.json
pomi content items render <menu-id> --version draft --display-type Detail
pomi content items publish <menu-id>
```

Use `DisplayText` as a link item's visible name. Use application-relative `~/`
URLs for tenant-aware internal links. Assign every embedded menu item a unique,
stable `ContentItemId`; only the root item receives a generated ID from the
content create endpoint.

For a content link, use the exact schema shape:

```json
{
  "ContentType": "ContentMenuItem",
  "ContentItemId": "b4dfc4bb-e6bc-40ca-a39b-5dffa9a83c1a",
  "DisplayText": "About",
  "ContentMenuItemPart": {
    "CheckContentPermissions": true,
    "SelectedContentItem": {
      "ContentItemIds": ["about-content-item-id"]
    }
  }
}
```

Prefer `ContentMenuItem` over copying a content item's current URL. Use
`LinkMenuItem` for external, fragment, or non-content routes. Use
`HtmlMenuItem` only when its sanitization policy and live schema fit the
requirement.

## Update safely

Menu updates merge supplied properties, but replace each supplied menu-item array:

```bash
pomi content items show <menu-id> --version draft > main-menu-current.json
pomi content items validate-update <menu-id> --body-file main-menu-updated.json
pomi content items update-draft <menu-id> --body-file main-menu-updated.json
pomi content items publish <menu-id>
```

Preserve nested item IDs and unknown properties returned by the API. Replacing
the menu without an existing nested item can create a new embedded identity and
discard its prior state.

## Render with the official menu shape

Render the menu by alias from a Liquid Layout or template:

```liquid
{% shape "menu",
  alias: "alias:main-menu",
  cache_id: "main-menu",
  cache_fixed_duration: "00:05:00",
  cache_tag: "alias:main-menu",
  cache_context: "user" %}
```

Do not load the Menu content item and manually iterate its raw JSON in a layout.
The official `menu` shape builds `MenuItem` children, resolves content links,
applies permissions, creates alternates, and participates in caching.

Vary cached output by `user` when content permissions can differ per identity,
including owner-based permissions. Use `user.roles` only when every visibility
decision is strictly role-based.

## Customize official shape templates

Use custom Liquid templates without bypassing the shape pipeline:

- `Menu` or `Menu__MainMenu`
- `MenuItem`
- `MenuItem__MainMenu`
- `MenuItem__LinkMenuItem`
- `MenuItem__MainMenu__LinkMenuItem`
- `MenuItem__level__2`
- corresponding `MenuItemLink...` alternates

Example `Menu__MainMenu` template:

```liquid
<nav id="mainNav" aria-label="{{ Model.Differentiator }}">
  <ul class="site-nav {{ Model.Classes | join: " " }}">
    {% for item in Model.Items %}
      {{ item | shape_render }}
    {% endfor %}
  </ul>
</nav>
```

Example base `MenuItem` template:

```liquid
<li class="site-nav__item{% if Model.HasItems %} has-children{% endif %}">
  {% shape_clear_alternates Model %}
  {% shape_type Model "MenuItemLink" %}
  {{ Model | shape_render }}
  {% if Model.HasItems %}
    <ul class="site-nav__children">
      {% for item in Model.Items %}
        {{ item | shape_render }}
      {% endfor %}
    </ul>
  {% endif %}
</li>
```

Inspect `pomi templates schema --operation create`, then use
`pomi templates create --body-file <file>` or
`pomi templates update <name> --body-file <file>`. Keep `MenuItemLink`
responsible for producing the anchor so content links, targets, and URL
normalization remain centralized.

## Active links and verification

Register Orchard's active-link script when the selected theme needs client-side
active navigation:

```liquid
{% script name:"menu-active-links", at:"Foot", depends_on:"jQuery",
  src:"~/OrchardCore.Menu/Scripts/activate-links.min.js",
  debug_src:"~/OrchardCore.Menu/Scripts/activate-links.js" %}

{% scriptblock name:"activate-main-menu", at:"Foot", depends_on:"menu-active-links" %}
  (function ($) {
    $("#mainNav").activateLinks({ selector: "a", traverse: 2 });
  })(jQuery);
{% endscriptblock %}
```

The active Layout must render `{% resources type:"FootScript" %}` after these
registrations or neither script executes.

Verify the root and representative content routes, nested keyboard navigation,
anchor targets, role-varying visibility, mobile/desktop layouts, script/CSS
requests, and browser console.

Official reference (live tenant schemas take precedence):
[Menu module](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/Menu/README.md).
