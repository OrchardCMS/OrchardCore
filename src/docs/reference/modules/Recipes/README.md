# Orchard Core Recipe Reference Guide

## Overview

The `OrchardCore.Recipes` module allows you to automate Orchard Core tenant setup and configuration using JSON-based recipe files. Recipes can install features, set themes, create content types, provision content, and much more.

---

## What is a Recipe?

A recipe is a `.recipe.json` file placed in a Recipes folder within your module or theme. Recipes are picked up by Orchard Core and can be executed via the admin panel or automatically during tenant setup.

### Key Properties

| Property         | Type     | Description                                                                 |
|------------------|----------|-----------------------------------------------------------------------------|
| `name`           | string   | The unique internal name of the recipe. Used for identifying the recipe in code. |
| `displayName`    | string   | The friendly name shown in the admin UI during setup.                      |
| `description`    | string   | A short description of what the recipe does. Displayed in the setup UI.    |
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
| `js:`          | `"ContentItemId": "[js:variables('homePageId')]"`      | Evaluates a JavaScript expression. Common for referencing `variables`.      |
| `file:`        | `"Content": "[file:text('Snippets/homepage.liquid')]"` | Loads content from an external file. Often used for Liquid templates.       |
| `env:`         | `"value": "[env:MyEnvironmentVariable]"`               | Injects values from environment variables.                                  |
| `appsettings:` | `"value": "[appsettings:OrchardCore:SiteName]"`        | Reads configuration from `appsettings.json`.                                |
| `localization:`| `"value": "[localization:WelcomeTitle]"`               | Retrieves localized strings by key.                                         |
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
