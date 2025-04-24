# Recipes (`OrchardCore.Recipes`)

## Overview

The `OrchardCore.Recipes` module allows you to automate Orchard Core tenant setup and configuration using JSON-based recipe files. Recipes can install features, set themes, create content types, provision content, and much more.

---

## What is a Recipe?

A recipe is a `.recipe.json` file placed in a Recipes folder within your module or theme. Recipes are picked up by Orchard Core and can be executed via the admin panel or automatically during tenant setup.

### Key Properties

| Property         | Type     | Description                                                                 |
|------------------|----------|-----------------------------------------------------------------------------|
| `name`           | string   | The unique internal name of the recipe. Used for identifying the recipe in code, including when executing it from other recipes. |
| `displayName`    | string   | The friendly name shown in the admin UI or during setup.                      |
| `description`    | string   | A short description of what the recipe does. Displayed in the admin and setup UIs.    |
| `author`         | string   | The name of the recipe creator or organization.                            |
| `website`        | string   | URL to the website or documentation for the recipe.                        |
| `version`        | string   | Semantic version (e.g., `1.0.0`) representing the recipe version.           |
| `issetuprecipe`  | boolean  | Indicates if this recipe should be available during tenant setup.          |
| `tags`           | array    | Keywords for categorizing the recipe in the UI (e.g., `["blog", "theme"]`). |
| `variables`      | object   | Key-value pairs to define reusable values throughout the recipe.           |
| `steps`          | array    | An ordered list of step objects that define the actions the recipe will perform. Each step has a `name` and step-specific parameters. |

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
    ...
  ]
}
```


## Recipe Helpers

These helpers allow dynamic expressions inside recipe values using a special syntax.

| Helper         | Example Usage                                          | Description                                                                 |
|----------------|--------------------------------------------------------|-----------------------------------------------------------------------------|
| `js`          | `"ContentItemId": "[js:variables('homePageId')]"`      | Evaluates a JavaScript expression. Common for referencing `variables`.      |
| `file`        | `"Content": "[file:text('Snippets/homepage.liquid')]"` | Loads content from an external file. Often used for Liquid templates.       |
| `env`         | `"value": "[env:MyEnvironmentVariable]"`               | Injects values from environment variables.                                  |
| `appsettings` | `"value": "[appsettings:OrchardCore:SiteName]"`        | Reads configuration from `appsettings.json`.                                |
| `localization`| `"value": "[localization:WelcomeTitle]"`               | Retrieves localized strings by key.                                         |
| `uuid()`       | `"Id": "[js:uuid()]"`                                  | Generates a new unique identifier (UUID/GUID).                              |

---

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
  }
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
{
  "name": "lucene-index",
  "Indices": [ { "Search": { ... } } ]
}
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

Defines deployment plans to export/import content and settings.

```json
{
  "name": "deployment",
  "Plans": [
    {
      "Name": "ExportSite",
      "Steps": [ ... ]
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

Let's say we want to deploy media assets as part of a module. Here’s how we’d structure this:

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
