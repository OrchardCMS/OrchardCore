# Recipes (`OrchardCore.Recipes`)

## Overview

The `OrchardCore.Recipes` module allows you to automate Orchard Core tenant setup and configuration using JSON-based recipe files. Recipes can install features, set themes, create content types, provision content, and much more.

---

## What is a Recipe?

A recipe is a `.recipe.json` file of a module, a theme, or the application, [registered](#registering-recipes) by a feature. Recipes can be executed via the admin panel, from other recipes, or automatically during tenant setup.

### Key Properties

| Property        | Type    | Description                                                                                                                           |
|-----------------|---------|---------------------------------------------------------------------------------------------------------------------------------------|
| `name`          | string  | The unique internal name of the recipe. Used for identifying the recipe in code, including when executing it from other recipes.      |
| `displayName`   | string  | The friendly name shown in the admin UI or during setup.                                                                              |
| `description`   | string  | A short description of what the recipe does. Displayed in the admin and setup UIs.                                                    |
| `author`        | string  | The name of the recipe creator or organization.                                                                                       |
| `website`       | string  | URL to the website or documentation for the recipe.                                                                                   |
| `version`       | string  | Semantic version (e.g., `1.0.0`) representing the recipe version.                                                                     |
| `issetuprecipe` | boolean | Indicates if this recipe should be available during tenant setup.                                                                     |
| `tags`          | array   | Keywords for categorizing the recipe in the UI (e.g., `["blog", "theme"]`).                                                           |
| `variables`     | object  | Key-value pairs to define reusable values throughout the recipe.                                                                      |
| `steps`         | array   | An ordered list of step objects that define the actions the recipe will perform. Each step has a `name` and step-specific parameters. |

### Example:

```json
{
  "name": "Blog",
  "displayName": "Blog Site",
  "description": "Creates a simple blog with custom content types, widgets, and pages.",
  "author": "Orchard Core Team",
  "website": "https://orchardcore.net",
  "version": "1.0.0",
  "issetuprecipe": true,
  "tags": [ "blog", "content", "theme" ],
  "variables": {
    "siteId": "[js:uuid()]"
  },
  "steps": [
    // Here you can add your steps which will be executed in the provided order.
  ]
}
```

!!! note
    Recipes, despite being JSON files, may contain comments: `// This is a comment.`

## Registering Recipes

A recipe file is registered with `AddRecipe()` from the startup of a feature. The path is relative to the root of the module or theme holding the file, whose files are all embedded in its assembly.

```csharp
[Feature("MyCompany.Blog.Samples")]
public sealed class SamplesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipe("Recipes/Samples/blog-samples.recipe.json");
    }
}
```

A registered recipe is only available to the tenants where the feature registering it is enabled. For instance, the `MenuAddPermissions` recipe of the `OrchardCore.Menu` module is only listed on the **Configuration** > **Recipes** admin page of the tenants where the Menu feature is enabled, and grouped under this feature.

- The file must not be located directly in the `Recipes` folder of the extension, since this folder is scanned regardless of the enabled features (see [Recipes Folders](#recipes-folders)). Put it in a sub folder instead, e.g. `Recipes/Samples/`. A registered recipe found directly in the `Recipes` folder is ignored, and a warning is logged.
- `AddRecipe(path, extensionId)` registers a recipe file of another extension, which is then available while the calling feature is enabled. This allows a feature to expose the setup recipes of a theme, for instance.
- When the application registers a recipe outside of a feature startup, for instance on the host service collection, the path is relative to the content root of the application.
- A missing or invalid recipe file is ignored, and the error is logged.

### Setup Recipes

The setup screen of a tenant, its AutoSetup, and the recipe lists of the Tenants module run in the setup shell of the tenant. This shell is only composed of the setup features of the application, added with `AddSetupFeatures()`, of its global features, added with `AddGlobalFeatures()`, and of their dependencies. So a setup recipe, one with `"issetuprecipe": true`, has to be registered by one of these features to be offered when setting up a tenant.

This lets each application decide which setup recipes it offers, for instance a different set per environment:

```csharp
builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("MyCompany.Recipes.Marketing");
```

A setup feature allowed on the Default tenant only, with `DefaultTenantOnly = true`, is not part of the setup shell of the other tenants, so its setup recipes are only offered when setting up the Default tenant.

When creating a tenant, the Tenants module lists the setup recipes that the setup screen of this tenant would offer. Use `ISetupService.GetSetupRecipesAsync(shellSettings)` to do the same from your own code.

### Default Tenant Recipes

The `OrchardCore.Recipes.Default` feature provides the recipes that are only available to the Default tenant, like the **SaaS** setup recipe used to set up a multi-tenant site. It is allowed on the Default tenant only, added as a setup feature by `AddOrchardCms()`, and enabled by the SaaS recipe.

### Recipes Folders

The `.recipe.json` files located directly in the `Recipes` folder of a module or theme, or in the `Recipes` folder of the application content root, are still found by convention. These recipes are available to every tenant, regardless of the features enabled on it, and every setup recipe found this way is offered when setting up any tenant.

This convention is kept for backward compatibility. Prefer registering your recipes with `AddRecipe()`, so that their availability depends on your features. To migrate a recipe, move its file to a sub folder, e.g. from `Recipes/blog.recipe.json` to `Recipes/Setup/blog.recipe.json`, and register it from the startup of the feature it belongs to, or from a setup feature for a setup recipe.


## Recipe Helpers

These helpers allow dynamic expressions inside recipe values using a special syntax.

| Helper         | Example Usage                                                                       | Description                                                                                                            |
|----------------|-------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------|
| `js`           | `"ContentItemId": "[js:variables('homePageId')]"`                                   | Evaluates a JavaScript expression. Common for referencing `variables`.                                                 |
| `file`         | `"Content": "[file:text('Snippets/homepage.liquid')]"`                              | Loads content from an external file. Often used for Liquid templates.                                                  |
| `env`          | `"value": "[env:MyEnvironmentVariable]"`                                            | Injects values from environment variables.                                                                             |
| `appsettings`  | `"value": "[appsettings:OrchardCore:SiteName]"`                                     | Reads configuration from `appsettings.json`.                                                                           |
| `localization` | `"value": "[localization:WelcomeTitle]"`                                            | Retrieves localized strings by key.                                                                                    |
| `uuid`         | `"Id": "[js:uuid()]"`                                                               | Generates a new unique identifier (UUID/GUID).                                                                         |
| `base64`       | `"data": "[js:base64('ew0KICAgICJ0eXBlIjogIkNvbnRlbnRJdGVtL0Jsb2dQb3N0Ig0KfQ==')]"` | Decodes the specified string from Base64 encoding. Use https://www.base64-image.de/ to convert your files to base64.   |
| `html`         | `"html": "[js:html('&lt;p&gt;Hello &amp; welcome&lt;/p&gt;')]"`                     | Decodes the specified string from HTML encoding.                                                                       |
| `gzip`         | `"data": "[js:gzip('data')]"`                                                       | Decodes the specified string from gzip/base64 encoding. Use http://www.txtwizard.net/compression to gzip your strings. |

---

## Custom Recipes

To create a new recipe step, implement the `IRecipeStepHandler` interface and its `ExecuteAsync` method:

```csharp
public async Task ExecuteAsync(RecipeExecutionContext context)
```

Alternatively, you can extend `NamedRecipeStepHandler` and implement the required abstract method, providing additional functionality for named steps.

## Built-in Recipe Steps

Each step is a JSON object in the `steps` array. Here are all built-in types:

### `feature`

Enables or disables features (modules/themes).

```json
{
  "name": "feature",
  "enable": [ "OrchardCore.Admin", "MyCustomTheme" ],
  "disable": []
}
```

!!! warning
    If you want to use your own theme (e.g., `YourTheme`), make sure to enable its feature; otherwise, the theme layout will not work after the recipe is executed.

---

### `themes`

Sets the active frontend and admin themes.

```json
{
  "name": "themes",
  "admin": "TheAdmin",
  "site": "MyCustomTheme"
}
```

---

### `settings`

Configures core site settings (like homepage route, culture, time zone, etc).

```json
{
  "name": "settings",
  "HomeRoute": {
    "Action": "Display",
    "Controller": "Item",
    "Area": "OrchardCore.Contents",
    "ContentItemId": "[js:variables('homeId')]"
  },
  "LayerSettings": {
    "Zones": [ "Content", "Footer" ]
  }
  // You may add other settings here
}
```

---

### `ContentDefinition`

Defines or updates content types and content parts.

```json
{
  "name": "ContentDefinition",
  "ContentTypes": [ { "Name": "Article", ... } ],
  "ContentParts": [ { "Name": "BodyPart", ... } ]
}
```

---

### `lucene-index`

Creates or configures Lucene search indexes.

```json
[
  {
    // Create the indices before the content items so they are indexed automatically.
    "name": "lucene-index",
    "Indices": [
      {
        "Search": {
          "AnalyzerName": "standardanalyzer",
          "IndexLatest": false,
          "IndexedContentTypes": [
            "Blog",
            "BlogPost"
          ]
        }
      }
    ]
  },
  {
    // Create the search settings.
    "name": "settings",
    "LuceneSettings": {
      "SearchIndex": "Search",
      "DefaultSearchFields": [
        "Content.ContentItem.FullText"
      ]
    }
  }
]
```

---

### `lucene-index-reset`

Clears index content.

```json
{
  "name": "lucene-index-reset",
  "includeAll": true
}
```

The `includeAll` property indicates whether to include all available Lucene indices. When set to `true`, the `Indices` property can be omitted.

---

### `lucene-index-rebuild`

Rebuilds Lucene indexes to reflect current content.

```json
{
  "name": "lucene-index-rebuild",
  "Indices": [ "Search" ]
}
```

---

### `content`

Imports content items such as pages, blogs, or menus.

```json
{
  "name": "content",
  "Data": [ { "ContentType": "Page", "DisplayText": "About", ... } ]
}
```

!!! note
    There is also `QueryBasedContentDeploymentStep` which produces exactly the same output as the Content Step, but based on a provided Query.

---

### `media`

Uploads files into the Media library.

```json
{
  "name": "media",
  "Files": [
    { "TargetPath": "logo.jpg", "SourcePath": "../wwwroot/img/logo.jpg" }
  ]
}
```

---

### `layers`

Defines layer rules for conditional widget placement.

```json
{
  "name": "layers",
  "Layers": [
    { "Name": "Always", "Rule": "true" },
    { "Name": "Homepage", "Rule": "isHomepage()" }
  ]
}
```

---

### `queries`

Adds Lucene or SQL queries to be reused by widgets or APIs.

```json
{
  "name": "queries",
  "Queries": [
    {
      "Source": "Lucene",
      "Name": "RecentPosts",
      "Index": "Search",
      "Template": "[file:text('Snippets/recentPosts.json')]",
      "ReturnContentItems": true
    }
  ]
}
```

---

### `AdminMenu`

Defines items in the admin menu for organizing admin tools.

```json
{
  "name": "AdminMenu",
  "data": [
    {
      "Id": "[js:uuid()]",
      "Name": "Tools",
      "MenuItems": [ ... ]
    }
  ]
}
```

---

### `Roles`

Creates user roles and assigns permissions.

```json
{
  "name": "Roles",
  "Roles": [
    {
      "Name": "Editor",
      "Permissions": [ "EditOwnContent", "PublishContent" ]
    }
  ]
}
```

---

### `Templates`

Defines or updates Liquid templates.

```json
{
  "name": "Templates",
  "Templates": {
    "Content__LandingPage": {
      "Description": "Landing page layout",
      "Content": "[file:text('Snippets/landingpage.liquid')]"
    }
  }
}
```

---

### `WorkflowType`

Defines custom workflows to automate user or content events.

```json
{
  "name": "WorkflowType",
  "data": [
    {
      "WorkflowTypeId": "[js:variables('workflowTypeId')]",
      "Name": "User Registration"
    }
  ]
}
```

---

### `deployment`

Defines deployment plans to export/import content and settings. Also see [Deployment](../Deployment/README.md).

```json
{
  "name": "deployment",
  "Plans": [
    {
      "Name": "ExportSite",
      "Steps": [
        {
          "Type": "CustomFileDeploymentStep",
          "Step": {
            "FileName": "Export",
            "FileContent": "Export",
            "Id": "[js: uuid()]",
            "Name": "CustomFileDeploymentStep"
          }
        },
        {
          "Type": "AllContentDeploymentStep",
          "Step": {
            "Id": "[js: uuid()]",
            "Name": "AllContent"
          }
        }
      ]
    }
  ]
}
```

---

### `custom-settings`

Updates content-based settings stored in a custom content item.

```json
{
  "name": "custom-settings",
  "MySiteSettings": {
    "ContentType": "MySiteSettings",
    "MySettingsPart": {
      "SomeTextField": { "Text": "Hello World" }
    }
  }
}
```

---

### `recipes`

Runs additional recipes within the current one, allowing modular reuse.

```json
{
  "name": "recipes",
  "Values": [
    { "executionid": "MyApp", "name": "MyApp.Pages" }
  ]
}
```
As `executionid` use a custom identifier to distinguish these recipe executions from others. As `name` use the `name` field from the given recipe's head (this is left blank when you export to recipes).

---

### `command`

Runs one or more of the built-in commands. This is useful during setup or import, for example to create a user.

```json
{
  "name": "command",
  "Commands": [
    "createUser /UserName:admin /Password:Password1! /Email:admin@example.com /Roles:Administrator"
  ]
}
```

Each entry in `Commands` is a single command: the command name followed by any `/Switch:value` arguments.

The command most useful in recipes is `createUser`, which provisions a user during setup or import. See the [Users documentation](../Users/README.md#commands) for its switches.

Other commands, such as `help commands` (lists every available command) and `recipes harvest` (lists the available recipes), write their result to the log and are meant for interactive or diagnostic use rather than for imports. Run `help commands` to see the full list of commands registered by the enabled features.

---

## Recipe Migrations

**Recipe migrations** allow you to perform updates using Orchard Core recipe files. These migrations are especially useful for updating metadata such as content types, workflows, settings, or any other component that can be updated via a recipe.

While many changes can be made through the admin UI, recipe migrations provide a repeatable and versioned way to apply updates, ideal for deployment automation or environment setup.

---

### Basic Concept

A recipe migration is implemented by creating a `DataMigration` class in your module or theme. Inside this class, you call into the `IRecipeMigrator` service to execute recipe files.

Recipe files must be stored in a `Migrations` folder within your project, and they are typically written in JSON format using the standard Orchard recipe schema.

---

### Setup

1. **Create a migration class**:
   - Inherit from `OrchardCore.Data.Migration.DataMigration` (in the `OrchardCore.Data.Abstractions` package).
   - Inject the `IRecipeMigrator` service.
   - Implement one or more of the following methods:
     - `CreateAsync()` – the first migration, must return `1`
     - `UpdateFrom<version>Async()` – used for incremental migrations

2. **Create migration recipe files**:
   - Place them in a `Migrations` folder (same level as your migration class).
   - Name them clearly to reflect the version or purpose.

---

### Example: Media Asset Migration

Let's say we want to deploy media assets as part of a module. Here's how we'd structure this:

#### Migration Class

```csharp
public sealed class Migrations : DataMigration
{
    private readonly IRecipeMigrator _recipeMigrator;

    public Migrations(IRecipeMigrator recipeMigrator)
    {
        _recipeMigrator = recipeMigrator;
    }

    public async Task<int> CreateAsync()
    {
        await _recipeMigrator.ExecuteAsync("migration.recipe.json", this);
        return 1;
    }

    public async Task<int> UpdateFrom1Async()
    {
        await _recipeMigrator.ExecuteAsync("migrationV2.recipe.json", this);
        return 2;
    }
}
```

!!! note 
    **Important**: Method names like `UpdateFrom1Async()` are **case-sensitive** and must follow the naming convention exactly in order to be discovered and executed.

---

### Recipe Files

Place the following JSON files in a folder named `Migrations`.

#### **Migrations/migration.recipe.json**

Initial migration adds two media files:

```json
{
  "steps": [
    {
      "name": "media",
      "Files": [
        {
          "TargetPath": "about/1.jpg",
          "SourcePath": "../wwwroot/img/about/1.jpg"
        },
        {
          "TargetPath": "about/2.jpg",
          "SourcePath": "../wwwroot/img/about/2.jpg"
        }
      ]
    }
  ]
}
```

#### **Migrations/migrationV2.recipe.json**

Second migration adds another image:

```json
{
  "steps": [
    {
      "name": "media",
      "Files": [
        {
          "TargetPath": "about/1.jpg",
          "SourcePath": "../wwwroot/img/about/1.jpg"
        },
        {
          "TargetPath": "about/2.jpg",
          "SourcePath": "../wwwroot/img/about/2.jpg"
        },
        {
          "TargetPath": "about/3.jpg",
          "SourcePath": "../wwwroot/img/about/3.jpg"
        }
      ]
    }
  ]
}
```

---

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/uJobH9izfLI" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/qPCBgHQYz1g" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/A13Li0CblK8" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/2c5pbXuJJb0" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
