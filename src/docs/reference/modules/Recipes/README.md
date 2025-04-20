# 📦 OrchardCore.Recipes

The `OrchardCore.Recipes` module provides a powerful way to automate configuration, setup, and content provisioning for Orchard Core tenants and modules using **recipe files** (JSON format).

---

## 📁 What is a Recipe?

A **recipe** is a `.recipe.json` file that contains one or more steps to configure a tenant—install features, create content types, import content, set themes, etc.

Place your recipe files in a folder named `Recipes` inside your module or theme. They will appear in the **Admin UI** under **Configuration > Recipes**.

File naming convention:
```
{RecipeName}.recipe.json
```

---

## 🧬 Recipe File Structure

A typical recipe file looks like this:

```json
{
  "name": "MyRecipe",
  "displayName": "My Custom Setup",
  "description": "Installs content, features, and config.",
  "author": "Dev Team",
  "website": "https://example.com",
  "version": "2.0",
  "issetuprecipe": true,
  "categories": [ "default" ],
  "tags": [ "blog", "landing" ],
  "variables": {
    "siteId": "[js:uuid()]"
  },
  "steps": [
    ...
  ]
}
```

### 🔍 Key Properties

| Property         | Description                                                                 |
|------------------|-----------------------------------------------------------------------------|
| `name`           | Internal name of the recipe.                                                |
| `displayName`    | Display name shown in the admin panel.                                     |
| `description`    | Description shown in the admin panel.                                      |
| `issetuprecipe`  | Set to `true` to make this recipe available during **tenant setup**.       |
| `variables`      | Define dynamic values (e.g., UUIDs) to reuse in steps.                     |
| `steps`          | Array of recipe steps to execute.                                          |

> 💡 **Note:** Recipes support JavaScript-based variable helpers (see [Recipe Helpers](#recipe-helpers) below).  
> ✅ You can include `// comments` in recipe JSON files—OrchardCore uses a relaxed JSON parser.

---

## 🧱 Implementing Custom Recipe Steps

To create a custom recipe step:

1. Implement `IRecipeStepHandler` and its `ExecuteAsync` method:
   ```csharp
   public Task ExecuteAsync(RecipeExecutionContext context)
   ```

2. Or inherit from `NamedRecipeStepHandler` to create a reusable step handler for a specific named step.

---

## 🧩 Built-in Recipe Steps

Here's a list of supported steps and usage examples:

### ✅ Feature Step
Enable or disable OrchardCore features (modules or themes):

```json
{
  "name": "feature",
  "enable": [ "OrchardCore.Admin", "MyCustomTheme" ],
  "disable": []
}
```

> ⚠️ Don’t forget to enable your custom theme if you're using one.

---

### 🎨 Themes Step
Set admin and frontend themes:

```json
{
  "name": "themes",
  "admin": "TheAdmin",
  "site": "MyCustomTheme"
}
```

---

### ⚙️ Settings Step
Configure system settings:

```json
{
  "name": "settings",
  "HomeRoute": {
    "Action": "Display",
    "Controller": "Item",
    "Area": "OrchardCore.Contents",
    "ContentItemId": "[js:variables('blogContentItemId')]"
  },
  "LayerSettings": {
    "Zones": [ "Content", "Footer" ]
  }
}
```

---

### 📐 ContentDefinition Step
Define content types and parts:

```json
{
  "name": "ContentDefinition",
  "ContentTypes": [ { "Name": "Article", ... } ],
  "ContentParts": [ { "Name": "BodyPart", ... } ]
}
```

---

### 🔍 Lucene Index Steps

#### Create Index:
```json
{
  "name": "lucene-index",
  "Indices": [ { "Search": { ... } } ]
}
```

#### Reset Index:
```json
{
  "name": "lucene-index-reset",
  "includeAll": true
}
```

#### Rebuild Index:
```json
{
  "name": "lucene-index-rebuild",
  "includeAll": false,
  "Indices": [ "Index1", "Index2" ]
}
```

---

### 📝 Content Step
Import content items:

```json
{
  "name": "content",
  "Data": [ { "ContentType": "Menu", ... } ]
}
```

---

### 📁 Media Step
Copy media files to tenant's Media folder:

```json
{
  "name": "media",
  "Files": [
    { "TargetPath": "logo.jpg", "SourcePath": "../wwwroot/img/logo.jpg" }
  ]
}
```

---

### 🎯 Layers Step
Define visibility rules for widgets:

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

### 📊 Queries Step
Create reusable Lucene or SQL queries:

```json
{
  "name": "queries",
  "Queries": [
    {
      "Source": "Lucene",
      "Name": "RecentBlogPosts",
      "Index": "Search",
      "Template": "[file:text('Snippets/recentBlogPosts.json')]",
      "ReturnContentItems": true
    }
  ]
}
```

---

### 🧭 AdminMenu Step
Define custom menus in the admin panel:

```json
{
  "name": "AdminMenu",
  "data": [
    {
      "Id": "[js:uuid()]",
      "Name": "My Admin Menu",
      "MenuItems": [ ... ]
    }
  ]
}
```

---

### 👥 Roles Step
Define roles and permissions:

```json
{
  "name": "Roles",
  "Roles": [
    {
      "Name": "Anonymous",
      "Permissions": [ "ViewContent", "QueryLuceneSearchIndex" ]
    }
  ]
}
```

> ⚠️ As of v1.6, **default roles are not created automatically**. Define them explicitly in your setup recipe.

---

### 🧪 Templates Step

Create Liquid templates:

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

### 🔄 Workflow Step
Provision workflows:

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

### 📦 Deployment Step
Define deployment plans:

```json
{
  "name": "deployment",
  "Plans": [
    {
      "Name": "Export",
      "Steps": [ ... ]
    }
  ]
}
```

---

### ⚙️ CustomSettings Step

Initialize your own content-based settings:

```json
{
  "name": "custom-settings",
  "MyCustomSettings": {
    "ContentType": "MyCustomSettings",
    "MyCustomSettingsPart": {
      "MyTextField": { "Text": "Hello" }
    }
  }
}
```

---

### 🍱 Recipes Step

Execute other recipes modularly:

```json
{
  "name": "recipes",
  "Values": [
    { "executionid": "MyApp", "name": "MyApp.Pages" }
  ]
}
```

---

## 🔧 Recipe Helpers

Use helpers to inject dynamic values into your recipe:

| Helper            | Description                                                      |
|-------------------|------------------------------------------------------------------|
| `uuid()`          | Generates a unique ID                                            |
| `base64(string)`  | Decodes Base64                                                   |
| `html(string)`    | Decodes HTML-encoded content                                     |
| `gzip(string)`    | Decompresses gzip/Base64-encoded content                         |

Example:
```json
"ContentItemId": "[js:uuid()]"
```

---

## 🚀 Recipe Migrations

Use recipe migrations to **update content/config programmatically** via recipe files.

Steps:

1. Create a class inheriting from `DataMigration`.
2. Inject `IRecipeMigrator`.
3. Implement `CreateAsync` and versioned `UpdateFrom{version}Async` methods.

```csharp
public sealed class Migrations : DataMigration
{
    private readonly IRecipeMigrator _recipeMigrator;

    public Migrations(IRecipeMigrator recipeMigrator) => _recipeMigrator = recipeMigrator;

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

Your migration recipe files go in the `Migrations` folder of your project.

---

## 🎥 Videos

Watch these walkthroughs:

- [Orchard Core Recipes Intro](https://www.youtube.com/embed/uJobH9izfLI)
- [Modular Recipes](https://www.youtube.com/embed/qPCBgHQYz1g)
- [Custom Features](https://www.youtube.com/embed/A13Li0CblK8)
- [Recipe Migrations](https://www.youtube.com/embed/2c5pbXuJJb0)
