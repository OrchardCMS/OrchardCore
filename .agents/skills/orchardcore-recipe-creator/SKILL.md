---
name: orchardcore-recipe-creator
description: Creates OrchardCore setup recipes (.recipe.json) that provision a tenant on setup — enabling features, setting themes, defining content types, importing content, roles, and settings. Use when the user needs to create a setup recipe, a starter recipe, or automate tenant configuration.
---

# OrchardCore Setup Recipe Creator

This skill guides you through creating a **setup recipe** for OrchardCore. A setup recipe is a `.recipe.json` file that runs when a tenant is created, provisioning everything the site needs: enabled features, active themes, content definitions, content items, roles, and settings.

## Prerequisites

- OrchardCore repository (working directory)
- A module or theme to host the recipe (recipes live in a `Recipes/` folder)
- Basic knowledge of the features your site needs

## What makes a recipe a *setup* recipe

A setup recipe is a normal recipe with `"issetuprecipe": true` in its header. That flag makes it appear in the setup screen's recipe dropdown so a user can pick it when creating a tenant. Without it, the recipe is still executable from the admin (Configuration → Recipes) but won't show during setup.

## Setup Recipe Workflow

### Step 1: Choose the host and create the Recipes folder

Recipes are discovered from a `Recipes/` folder inside any enabled module or theme:

```bash
mkdir src/OrchardCore.Themes/YourTheme/Recipes
```

The file name must end with `.recipe.json`:

```
src/OrchardCore.Themes/YourTheme/Recipes/yoursite.recipe.json
```

### Step 2: Write the recipe header

```json
{
  "name": "YourSite",
  "displayName": "Your Site",
  "description": "Provisions a Your Site tenant with content and theme.",
  "author": "The Orchard Core Team",
  "website": "https://orchardcore.net",
  "version": "1.0.0",
  "issetuprecipe": true,
  "categories": [ "default" ],
  "tags": [ "yoursite" ],
  "variables": {
    "homeContentItemId": "[js:uuid()]"
  },
  "steps": []
}
```

`variables` are evaluated on first access and reused across steps — use them for content item ids so steps can reference the same item. Recipe JSON may contain `// comments`.

### Step 3: Add the core steps in order

Steps run **top to bottom**. A typical setup recipe order:

1. `feature` — enable the modules and theme features the site needs.
2. `themes` — set the active site and admin themes.
3. `settings` — site-wide settings (home route, culture, time zone).
4. `Roles` — roles and their permissions.
5. `ContentDefinition` — content types and parts.
6. `content` — the actual content items (pages, menus, widgets).
7. `media`, `queries`, `layers`, `Templates`, `AdminMenu` — as needed.

Minimum viable setup recipe:

```json
"steps": [
  {
    "name": "feature",
    "enable": [
      "OrchardCore.Admin",
      "OrchardCore.Contents",
      "OrchardCore.ContentTypes",
      "OrchardCore.Title",
      "OrchardCore.Autoroute",
      "OrchardCore.Alias",
      "OrchardCore.Themes",
      "OrchardCore.HomeRoute",
      "TheTheme",
      "YourTheme"
    ]
  },
  {
    "name": "themes",
    "admin": "TheAdmin",
    "site": "YourTheme"
  }
]
```

!!! warning
    Always enable your own theme's feature in the `feature` step. If you set it as the `site` theme without enabling it, the layout won't render after setup.

### Step 4: Reference the full step catalog

For every built-in step (`feature`, `themes`, `settings`, `ContentDefinition`, `content`, `media`, `Roles`, `layers`, `queries`, `Templates`, `AdminMenu`, `WorkflowType`, `deployment`, `custom-settings`, `recipes`, search indexes), see `references/steps.md`.

### Step 5: Use recipe helpers for dynamic values

Values can embed expressions with `[helper:...]` syntax:

| Helper | Example | Use |
|--------|---------|-----|
| `js` | `"[js:variables('homeContentItemId')]"` | Reference variables / JS expressions |
| `uuid` | `"[js:uuid()]"` | Generate a new content item id |
| `file` | `"[file:text('Snippets/home.liquid')]"` | Inline an external file's text |
| `env` | `"[env:MY_VAR]"` | Read an environment variable |
| `appsettings` | `"[appsettings:OrchardCore:SiteName]"` | Read appsettings config |
| `localization` | `"[localization:WelcomeTitle]"` | Localized string |
| `base64` / `gzip` | `"[js:base64('...')]"` | Decode embedded binary/text |

### Step 6: Test the recipe

```bash
# Build the host module/theme
dotnet build src/OrchardCore.Themes/YourTheme

# Run the application
cd src/OrchardCore.Cms.Web
dotnet run -f net10.0
```

Then create a new tenant (or reset the default one) and pick **Your Site** in the setup screen's recipe list. Verify features, theme, and content provisioned correctly.

To re-run a non-setup recipe without a fresh tenant: Admin → Configuration → Recipes → Run.

## Quick Reference

### Header properties

| Property | Purpose |
|----------|---------|
| `name` | Unique internal id (used by the `recipes` step to call it) |
| `displayName` | Friendly name in setup/admin UI |
| `description` | Short summary shown to the user |
| `issetuprecipe` | `true` makes it selectable during tenant setup |
| `categories` / `tags` | Grouping/filtering in the UI |
| `variables` | Reusable values, evaluated once on first access |
| `steps` | Ordered actions to execute |

### Common feature ids

| Area | Features |
|------|----------|
| Core | `OrchardCore.Admin`, `OrchardCore.HomeRoute`, `OrchardCore.Themes`, `OrchardCore.Settings`, `OrchardCore.Recipes` |
| Content | `OrchardCore.Contents`, `OrchardCore.ContentTypes`, `OrchardCore.ContentFields`, `OrchardCore.Title`, `OrchardCore.Autoroute`, `OrchardCore.Alias`, `OrchardCore.Html`, `OrchardCore.Flows`, `OrchardCore.Lists`, `OrchardCore.Widgets` |
| Media/Menu | `OrchardCore.Media`, `OrchardCore.Menu`, `OrchardCore.Layers` |
| Security | `OrchardCore.Users`, `OrchardCore.Roles`, `OrchardCore.Security` |

### Naming Conventions

| Item | Convention | Example |
|------|------------|---------|
| Recipe file | `name.recipe.json` (lowercase) | `blog.recipe.json` |
| `name` property | PascalCase | `Blog` |
| Location | `Recipes/` in a module or theme | `Themes/TheBlogTheme/Recipes/` |

## References

- `references/recipe-structure.md` - File layout, header, variables, helpers
- `references/steps.md` - Every built-in recipe step with examples
- `reference/modules/Recipes/README.md` (docs) - Full recipe documentation
- Example recipes: `src/OrchardCore.Themes/*/Recipes/*.recipe.json`
