# Recipes (OrchardCore.Recipes)

## Recipe helpers

Recipes can use script helpers like this:

```json
{
    "ContentItemId": "[js: uuid()]"
}
```

| Name | Description |
| --- | --- |
| `uuid()` | Generates a unique identifier for a content item. |
| `base64(string)` | Decodes the specified string from Base64 encoding. Use https://www.base64-image.de/ to convert your files to base64. |
| `html(string)` | Decodes the specified string from HTML encoding. |
| `gzip(string)` | Decodes the specified string from gzip/base64 encoding. Use http://www.txtwizard.net/compression to gzip your strings. |

## Recipe Migrations

A recipe migration is a way to perform updates via a recipe file. The most common uses for this would be to update metadata like content types or workflows, but one could update anything that is updateable via a recipe.

Let's consider a simple scenario: adding a new asset. Now one could do this via the admin UI interface, but the purpose here is to demonstrate all the moving parts involved in a recipe migration.

In your module or theme project, create a class that inherits from `OrchardCore.Data.Migration.DataMigration` (found in the `OrchardCore.Data.Abstractions` package). Add a dependency for the `IRecipeMigrator` service to this class. Provide `CreateAsync()` and/or `UpdateAsync()` methods that return `Task<int>` to provide the migration steps. The class can placed anywhere in your project, but the recipe JSON files must be placed in a folder named `Migrations`.

Here is an example of how initial and subsequent migrations can be authored. Use the `CreateAsync()` method to provide the very first migration that runs and ensure that this method always returns 1. Use the `UpdateFrom<version>Async()` to provide subsequent migrations; in this example, we have a migration that updates from version 1 to 2. The method names are case-sensitive and the naming convention must be followed for the migrations to be discovered and executed.

```csharp
public class Migrations : DataMigration
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

And here are the migration recipes referenced in the code above:

**Migrations/migration.recipe.json**

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
        }
    ]
}
```

**Migrations/migrationV2.recipe.json**

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
        }
    ]
}
```
