# Recipes (`OrchardCore.Recipes`)

## Recipe file

A recipe is a json file used to execute different import and configuration steps.

You can add it in a `Recipes` folder with a name like this `{RecipeName}.recipe.json` and it will be available in the Configuration > Recipes admin page.

## Recipe format

A recipe file should look like this:

```json
{
  "name": "Name",
  "displayName": "Display Name of the recipe",
  "description": "Description of the recipe",
  "author": "Author",
  "website": "https://website.net",
  "version": "2.0",
  "issetuprecipe": true|false,
  "categories": [ "default" ],
  "tags": [ "tag" ],
  "variables": {
    "variable1": "[js:uuid()]",
    ...
  },
  "steps": [
      ...
  ]
}
```

!!! note
    if `issetuprecipe` is equal to true, the recipe will be available in the Recipes list during the setup.

!!! note
    Recipes, despite being JSON files, may contain comments:
    ```json
    // This is a comment.
    ```

## Recipe steps

A recipe can execute multiple steps.

In order to create a new Recipe step, you need to implement the `IRecipeStepHandler` interface and the `ExecuteAsync` method:
`public async Task ExecuteAsync(RecipeExecutionContext context)`

Here are the available recipe steps:

### Feature Step

The Feature step allows you to disable/enable some features.

```json
    {
        "name": "feature",
        "disable": [],
        "enable": [
            "OrchardCore.Admin",
            "YourTheme",
            "TheAdmin"
        ]
    }
```

!!! warning
    If you want to use your own theme (Ex: `YourTheme`), do not forget to enable its feature or else, the theme layout will not be working after the execution of the recipe.

### Themes Step

The Themes step allows you to set the admin and the site themes.

```json
    {
      "name": "themes",
      "admin": "TheAdmin",
      "site": "YourTheme"
    }
```

### Settings Step

The Settings step allows you to set multiple settings.

```json
    {
      "name": "settings",
      "HomeRoute": {
        "Action": "Display",
        "Controller": "Item",
        "Area": "OrchardCore.Contents",
        "ContentItemId": "[js: variables('blogContentItemId')]"
      },
      "LayerSettings": {
        "Zones": [ "Content", "Footer" ]
      }
    }
```

### ContentDefinition Step

The Content step allows you to import some content types.

```json
    {
        "name": "ContentDefinition",
        "ContentTypes": [
        {
            "Name": "YourContentType",
            ...
        }
    }
```

### Lucene Step

The Lucene index step allows you to run the Lucene indexation of content types.  
You can also set the default Lucene Settings.

```json
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
      "name": "Settings",
      "LuceneSettings": {
        "SearchIndex": "Search",
        "DefaultSearchFields": [
          "Content.ContentItem.FullText"
        ]
      }
    }
```

### Content Step

The Content step allows you to create content items.

```json
     {
      "name": "content",
      "Data": [
        {
          "ContentType": "Menu",
          ...
        },
        ...
      ]
    }
```

!!! note
    There is also `QueryBasedContentDeploymentStep` which produces exactly the same output as the Content Step, but based on a provided Query.

### Media Step

The Media step allows you to import media files to the tenant Media folder.

```json
    {
      "name": "media",
      "Files": [
        {
          "TargetPath": "home-bg.jpg",
          "SourcePath": "../wwwroot/img/home-bg.jpg"
        },
        {
          "TargetPath": "post-bg.jpg",
          "SourcePath": "../wwwroot/img/post-bg.jpg"
        }
      ]
    }
```

### Layers Step

The Layers step allows you to create multiple layers.

```json
    {
      "name": "layers",
      "Layers": [
        {
          "Name": "Always",
          "Rule": "true",
          "Description": "The widgets in this layer are displayed on any page of this site."
        },
        {
          "Name": "Homepage",
          "Rule": "isHomepage()",
          "Description": "The widgets in this layer are only displayed on the homepage."
        }
      ]
    }
```

### Queries Step

The Queries step allows you to create multiple Lucene or SQL queries.

```json
    {
      "name": "queries",
      "Queries": [
        {
          "Source": "Lucene",
          "Name": "RecentBlogPosts",
          "Index": "Search",
          "Template": "[file:text('Snippets/recentBlogPosts.json')]",
          "Schema": "[js:base64('ew0KICAgICJ0eXBlIjogIkNvbnRlbnRJdGVtL0Jsb2dQb3N0Ig0KfQ==')]",
          "ReturnContentItems": true
        }
      ]
    }
```

### AdminMenu Step

The AdminMenu step allows you to create multiple admin menus.

```json
    {
      "name": "AdminMenu",
      "data": [
        {
          "Id": "[js:uuid()]",
          "Name": "Admin menu",
          "Enabled": true,
          "MenuItems": [
              ...
          ]
        }
      ]
    }
```

### Roles Step

The Roles step allows you to set permissions to specific roles.

```json
    {
      "name": "Roles",
      "Roles": [
        {
          "Name": "Anonymous",
          "Description": "Anonymous role",
          "Permissions": [
            "ViewContent",
            "QueryLuceneSearchIndex"
          ]
        }
      ]
    }
```

### Template and AdminTemplate Step

The Template and AdminTemplate steps allow you to create Liquid Templates.

```json
    {
      "name": "Templates",
      "Templates": {
        "Content__LandingPage": {
          "Description": "A template for the Landing Page content type",
          "Content": "[file:text('Snippets/landingpage.liquid')]"
        }
      }
    }
```

### Workflow Step

The WorkflowType step allows you to create a Workflow.

```json
    {
      "name": "WorkflowType",
      "data": [
        {
          "WorkflowTypeId": "[js: variables('workflowTypeId')]",
          "Name": "User Registration",
          ...
        }
      ]
    }
```

### Deployment Step

The Deployment step allows you to create a deployment plan with deployment steps. Also see [Deployment](../Deployment/README.md).

```json
    {
    "name": "deployment",
    "Plans": [
    {
        "Name": "Export",
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
```

### CustomSettings Step

The CustomSettings step allows you to populate your custom settings with initial values.

```json
    {
      "name": "custom-settings",
      "MyCustomSettings": {
        "ContentItemId": "400d6c7pwj8675crzacd6gyywt",
        "ContentItemVersionId": null,
        "ContentType": "MyCustomSettings",
        "DisplayText": "",
        "Latest": false,
        "Published": false,
        "ModifiedUtc": null,
        "PublishedUtc": null,
        "CreatedUtc": null,
        "Owner": "",
        "Author": "",
        "MyCustomSettingsPart": {
          "MyTextField": {
            "Text": "My custom text"
          }
        }
      }
    }
```

### Recipes Step

The Recipes step allows you to execute other recipes from the current recipe. You can use this to modularize your recipes. E.g. instead of having a single large setup recipe you can put content into multiple smaller ones and execute them from the setup recipe.

```json
    {
      "name": "recipes",
      "Values": [
        {
          "executionid": "MyApp",
          "name": "MyApp.Pages"
        },
        {
          "executionid": "MyApp",
          "name": "MyApp.Blog"
        }
      ]
    }
```

As `executionid` use a custom identifier to distinguish these recipe executions from others. As `name` use the `name` field from the given recipe's head (this is left blank when you export to recipes).

### Other settings Step

Here are other available steps:

- `Command`
- `FacebookLoginSettings`
- `FacebookSettings`
- `GitHubAuthentication`
- `GoogleAnalyticsSettings`
- `GoogleAuthenticationSettings`
- `AzureADSettings`
- `MicrosoftAccountSettings`
- `OpenIdApplication`
- `OpenIdClientSettings`
- `OpenIdServerSettings`
- `Twitter`

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
          ]
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
          ]
        }
    ]
}
```

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/uJobH9izfLI" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/qPCBgHQYz1g" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/A13Li0CblK8" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
