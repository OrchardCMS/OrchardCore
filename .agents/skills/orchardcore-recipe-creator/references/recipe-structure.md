# Recipe Structure Reference

## Where recipes live

A recipe is a `*.recipe.json` file in a `Recipes/` folder inside any module or theme. OrchardCore discovers it automatically once the host feature is enabled.

```
src/OrchardCore.Themes/YourTheme/
├── Manifest.cs
├── YourTheme.csproj
├── Recipes/
│   ├── yoursite.recipe.json        # Setup recipe (issetuprecipe: true)
│   └── yoursite.content.recipe.json # Optional add-on recipe
└── Snippets/                        # Optional: files referenced via [file:text(...)]
    └── home.liquid
```

Setup recipes (with `"issetuprecipe": true`) appear in the setup screen's recipe list. Non-setup recipes are runnable from Admin → Configuration → Recipes.

## Full header

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

  // Variables are evaluated on first access and reused across steps.
  "variables": {
    "homeContentItemId": "[js:uuid()]",
    "menuContentItemId": "[js:uuid()]",
    "adminUrlPrefix": "[js: configuration('OrchardCore_Admin:AdminUrlPrefix', 'Admin')]"
  },

  "steps": [
    // Steps execute in order.
  ]
}
```

| Property | Type | Description |
|----------|------|-------------|
| `name` | string | Unique internal name. Used to call the recipe from a `recipes` step. |
| `displayName` | string | Friendly name in setup/admin UI. |
| `description` | string | Short summary shown to the user. |
| `author` | string | Recipe creator. |
| `website` | string | Docs/site URL. |
| `version` | string | Semantic version, e.g. `1.0.0`. |
| `issetuprecipe` | bool | `true` → selectable during tenant setup. |
| `categories` | array | Grouping in the UI. |
| `tags` | array | Keywords for filtering. |
| `variables` | object | Reusable values, lazily evaluated once. |
| `steps` | array | Ordered list of step objects, each with a `name`. |

!!! note
    Recipe JSON allows `// comments` even though standard JSON does not.

## Variables

Use variables to share a generated id between steps — e.g. define a content item in `content`, then point the home route at it in `settings`:

```json
"variables": {
  "homeContentItemId": "[js:uuid()]"
}
```

```json
{
  "name": "settings",
  "HomeRoute": {
    "Action": "Display",
    "Controller": "Item",
    "Area": "OrchardCore.Contents",
    "ContentItemId": "[js:variables('homeContentItemId')]"
  }
}
```

## Recipe helpers

Embed dynamic values with `[helper:expression]`:

| Helper | Example | Description |
|--------|---------|-------------|
| `js` | `"[js:variables('homeContentItemId')]"` | Evaluate a JS expression; reference variables. |
| `uuid` | `"[js:uuid()]"` | Generate a new GUID. |
| `file` | `"[file:text('Snippets/home.liquid')]"` | Inline an external file's text. |
| `env` | `"[env:MY_VAR]"` | Read an environment variable. |
| `appsettings` | `"[appsettings:OrchardCore:SiteName]"` | Read from `appsettings.json`. |
| `localization` | `"[localization:WelcomeTitle]"` | Localized string by key. |
| `base64` | `"[js:base64('...')]"` | Decode a base64 string. |
| `gzip` | `"[js:gzip('...')]"` | Decode a gzip+base64 string. |
| `html` | `"[js:html('&lt;p&gt;Hi&lt;/p&gt;')]"` | HTML-decode a string. |

## Recipe migrations (advanced)

Recipes can also run as **migrations** from a `DataMigration` class via `IRecipeMigrator`. Those recipe files go in a `Migrations/` folder (not `Recipes/`) and are executed from `CreateAsync()` / `UpdateFromNAsync()`. Use this to version content-type or media changes for existing tenants. See the Recipes docs for the full pattern.
