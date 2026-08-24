# Content Types (`OrchardCore.ContentTypes`)

## View Components

### `SelectContentTypes`

Renders an editor to select a list of content types.  
It can optionally filter content types of a specific stereotype.  
The editor returns the selection as a `string[]` on the model.

#### Parameters

| Parameter               | Type       | Description                                                                            |
|-------------------------|------------|----------------------------------------------------------------------------------------|
| `selectedContentTypes`  | `string[]` | The list of content types that should be marked as selected when rendering the editor. |
| `htmlName`              | `string`   | The name of the model property to bind the result to.                                  |
| `stereotype` (optional) | `string`   | A stereotype name to filter the list of content types available to select.             |

#### Sample

```csharp
@await Component.InvokeAsync("SelectContentTypes", new { selectedContentTypes = Model.ContainedContentTypes, htmlName = Html.NameFor(m => m.ContainedContentTypes) })
```

## Migrations

Migration classes can be used to alter the content type definitions, like by adding new **types**, or configuring their **parts** and **fields**.

### `IContentDefinitionManager`

This service provides a way to modify the content type definitions. From a migrations class, we can inject an instance of this interface.

```csharp
public sealed class Migrations : DataMigration
{
    IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public Task<int> CreateAsync()
    {
        // This code will be run when the feature is enabled

        return 1;
    }
}
```

### Creating a new Content Type

The following example creates a new Content Type named `Product`.

```csharp
await _contentDefinitionManager.AlterTypeDefinitionAsync("Product");
```

### Changing the metadata of a Content Type

To change specific properties of the content type, an argument can be used to configure it:

```csharp
await _contentDefinitionManager.AlterTypeDefinitionAsync("Product", type => type
    // content items of this type can have drafts
    .Draftable()
    // content items versions of this type have saved
    .Versionable()
    // this content type appears in the New menu section
    .Creatable()
    // permissions can be applied specifically to instances of this type
    .Securable()
);
```

### Adding Content Parts to a type

The following example adds the `TitlePart` content part to the `Product` type.

```csharp
await _contentDefinitionManager.AlterTypeDefinitionAsync("Product", type => type
    .WithPart("TitlePart")
);
```

Each part can also be configured in the context of a type. For instance the `AutoroutePart` requires a **Liquid** template as its pattern to generate custom routes. It's defined in a custom setting for this part.

```csharp
await _contentDefinitionManager.AlterTypeDefinitionAsync("Product", type => type
    .WithPart("AutoroutePart", part => part
        // sets the position among other parts
        .WithPosition("2")
        // sets all the settings on the AutoroutePart
        .WithSettings(new AutoroutePartSettings { Pattern = "{{ ContentItem | display_text | slugify }}" })
    )
);
```

For a list of all the settings each type can use, please refer to their respective documentation pages.

### Adding Content Fields to a part

Fields can not be attached directly to a Content Type. To add fields to a content type, create a part with the same name as the type, and add fields to this part.

```csharp
await _contentDefinitionManager.AlterTypeDefinitionAsync("Product", type => type
    .WithPart("Product")
);

await _contentDefinitionManager.AlterPartDefinitionAsync("Product", part => part
    .WithField("Image", field => field
        .OfType("MediaField")
        .WithDisplayName("Main image")
    )
    .WithField("Price", field => field
        .OfType("NumericField")
        .WithDisplayName("Price")
    )
);
```

When added to a part, fields can also have custom settings which for instance will define how the editor will behave, or validation rules. Also refer to their respective documentation pages for a list of possible settings.

### Consuming Content Parts and Fields from CSharp

It's possible to get strongly typed versions of Content Parts and Fields from the above type definitions.

!!! warning
These types may be modified in the CMS. It's important to make sure these types will not be modified outside of the development cycle when consuming them in code.

First, create a part that matches the type definition:

```csharp
public class Product : ContentPart
{
    public MediaField Image { get; set; }
    public NumericField Price { get; set; }
}
```

Then, register your ContentPart with Dependency Injection:

```csharp
using OrchardCore.ContentManagement;

...

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPart<Product>();
    }
}
```

Finally, here is an example of consuming your Content Item as your Content Part in a Controller.

```csharp
public sealed class ProductController : Controller
{
    private readonly IOrchardHelper _orchardHelper;
    private readonly IContentManager _contentManager;

    public ProductController(IOrchardHelper orchardHelper, IContentManager contentManager)
    {
        _orchardHelper = orchardHelper;
        _contentManager = contentManager;
    }

    [HttpGet("/api/product/{productId}")]
    public async Task<ObjectResult> GetProductAsync(string productId)
    {
        var product = _orchardHelper.GetContentItemByIdAsync(productId);

        if (product == null)
        {
            return NotFoundObjectResult();
        }

        var productPart = product.GetOrCreate<Product>();

        // you'll get exceptions if any of these Fields are null
        // the null-conditional operator (?) should be used for any fields which aren't required
        return new ObjectResult(new {
             Image = productPart.Image.Paths.FirstOrDefault(),
             Price = productPart.Price.Value,
        });
    }

    [HttpPost("/api/product/{productId}/price/{price}")]
    public async Task<ContentValidateResult> UpdateProductPriceAsync(string productId, int price)
    {
        //this call will only fetch published content item, which makes publishing after update redundant
        var product = _orchardHelper.GetContentItemByIdAsync(productId);

        if (product == null)
        {
            return NotFoundObjectResult();
        }

        var productPart = product.GetOrCreate<Product>();
        productPart.Price.Value = price;

        product.Apply(productPart) //apply modified part to a content item

        await _contentManager.UpdateAsync(product); //update will fire handlers which could alter the content item.

        //validate the content item after update since handlers could change the object.
        var result = await _contentManager.ValidateAsync(product);

        if (!result.Succeeded)
        {
            // Cancel the session to discard any pending changes.
            await _session.CancelAsync();
        }

        return result;
    }
}
```

## Content Definition Handlers

The `IContentDefinitionHandler` interface allows you to intercept and modify content definitions as they are being built by the `IContentDefinitionManager`, before they are cached. This provides fine-grained control over the shape of content types, parts, and fields, and enables scenarios such as injecting parts into a content type programmatically.

!!! note
    `IContentDefinitionHandler` is invoked while the definition is being *built* (read) from its stored records. It is different from `IContentDefinitionEventHandler`, which is invoked when definitions are *altered* (created, updated, removed, imported).

The following methods are available:

| Method | Description |
|--------|-------------|
| `ContentTypeBuilding(ContentTypeBuildingContext context)` | Invoked while a content type is being built. |
| `ContentPartBuilding(ContentPartBuildingContext context)` | Invoked while a content part definition is being built. |
| `ContentTypePartBuilding(ContentTypePartBuildingContext context)` | Invoked while a part attached to a content type is being built. |
| `ContentPartFieldBuilding(ContentPartFieldBuildingContext context)` | Invoked while a field on a content part is being built. |

Each context exposes a mutable `Record` property that you can modify. Setting `context.Record` to `null` removes the corresponding type, part, or field from the built definition. When a part or field is requested but no record exists yet, `context.Record` is `null` on entry, allowing a handler to create a definition on demand.

### Registering a handler

Register your implementation with the dependency injection container from a `Startup` class:

```csharp
using OrchardCore.ContentTypes.Events;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentDefinitionHandler, MyContentDefinitionHandler>();
    }
}
```

## System-Defined Types, Parts, and Fields

Content definition handlers make it possible to designate a content type, part, or field as **system-defined**. A system-defined element is one that is required by a feature and must always be present with a consistent structure. System-defined elements:

- Cannot be removed or modified by users through the admin UI.
- Cannot be removed or altered through recipes.
- Can be injected programmatically so they are always part of the definition, even if they were never persisted.

An element is marked as system-defined by storing a `ContentSettings` with `IsSystemDefined` set to `true` in the `Settings` of its record, from within a content definition handler:

```csharp
using OrchardCore.ContentManagement.Metadata.Settings;

context.Record.Settings[nameof(ContentSettings)] = JObject.FromObject(new ContentSettings
{
    IsSystemDefined = true,
});
```

When reading a built definition, you can inspect the flag with `GetSettings<ContentSettings>()`, for example `typeDefinition.GetSettings<ContentSettings>().IsSystemDefined`.

When an element is system-defined, the admin UI hides the corresponding **Remove** action and displays a tooltip explaining that it is integral to the system. Attempting to remove a system-defined type, part, or field (for example through the `IContentDefinitionService`) throws an `InvalidOperationException`.

### Example: injecting a system-defined part

The `DashboardPart` is injected into every content type that uses the `DashboardWidget` stereotype, without the user having to attach it manually. This is implemented by the `DashboardPartContentTypeDefinitionHandler` in the `OrchardCore.AdminDashboard` module.

```csharp
public sealed class DashboardPartContentTypeDefinitionHandler : IContentDefinitionHandler
{
    // Adds the DashboardPart to the content type when the stereotype is 'DashboardWidget'.
    public void ContentTypeBuilding(ContentTypeBuildingContext context)
    {
        if (context?.Record?.Settings is null ||
            !context.Record.Settings.TryGetPropertyValue(nameof(ContentTypeSettings), out var node))
        {
            return;
        }

        var settings = node.ToObject<ContentTypeSettings>();

        if (!string.Equals(settings.Stereotype, "DashboardWidget", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Don't add the part twice.
        if (context.Record.ContentTypePartDefinitionRecords.Any(x => x.Name.EqualsOrdinalIgnoreCase(nameof(DashboardPart))))
        {
            return;
        }

        context.Record.ContentTypePartDefinitionRecords.Add(new ContentTypePartDefinitionRecord
        {
            Name = nameof(DashboardPart),
            PartName = nameof(DashboardPart),
            Settings = new JsonObject
            {
                [nameof(ContentSettings)] = JObject.FromObject(new ContentSettings
                {
                    IsSystemDefined = true,
                }),
            },
        });
    }

    // Ensures the part attached to the type stays marked as system-defined.
    public void ContentTypePartBuilding(ContentTypePartBuildingContext context)
    {
        if (context?.Record?.Settings is null || !context.Record.PartName.EqualsOrdinalIgnoreCase(nameof(DashboardPart)))
        {
            return;
        }

        var settings = context.Record.Settings[nameof(ContentSettings)]?.ToObject<ContentSettings>()
            ?? new ContentSettings();

        settings.IsSystemDefined = true;

        context.Record.Settings[nameof(ContentSettings)] = JObject.FromObject(settings);
    }

    // Creates the DashboardPart definition on demand when it has never been persisted.
    public void ContentPartBuilding(ContentPartBuildingContext context)
    {
        if (context.Record is not null || context.PartName != nameof(DashboardPart))
        {
            return;
        }

        context.Record = new ContentPartDefinitionRecord
        {
            Name = context.PartName,
            Settings = new JsonObject
            {
                [nameof(ContentPartSettings)] = JObject.FromObject(new ContentPartSettings
                {
                    Attachable = false,
                    Reusable = false,
                }),
                [nameof(ContentSettings)] = JObject.FromObject(new ContentSettings
                {
                    IsSystemDefined = true,
                }),
            },
        };
    }

    public void ContentPartFieldBuilding(ContentPartFieldBuildingContext context)
    {
    }
}
```

With this handler registered, assigning the `DashboardWidget` stereotype to a content type is enough to make it behave as an admin dashboard widget — the `DashboardPart` is always present, is marked as system-defined, and cannot be detached by the user.

## Content Type Settings for Block Pickers

Content types can be configured with a category and thumbnail for use in block picker modals (such as those used by the [Flows module](../Flow/README.md#blocks-editor)).

### Category

Content types can be organized into categories. To set a category:

1. Navigate to **Design** → **Content Definition** → **Content Types**
2. Edit the content type
3. In the **Content Type Settings** section, set the **Category** field

Or programmatically:

```csharp
_contentDefinitionManager.AlterTypeDefinition("MyWidget", type => type
    .WithCategory("Media")
);
```

Content types with the same category are grouped together in the picker's sidebar.

### Thumbnail

Content types can display a thumbnail image in block pickers. To set a thumbnail:

1. Navigate to **Design** → **Content Definition** → **Content Types**
2. Edit the content type
3. In the **Content Type Settings** section, set the **Thumbnail Path** field to an image path (e.g., `/media/thumbnails/my-widget.png`)

Or programmatically:

```csharp
_contentDefinitionManager.AlterTypeDefinition("MyWidget", type => type
    .WithThumbnailPath("/media/thumbnails/my-widget.png")
);
```

### Default Category and Thumbnail

Default values for category and thumbnail can be configured in `appsettings.json` using `ContentTypesOptions`:

```json
{
    "OrchardCore": {
        "OrchardCore_ContentTypes": {
            "DefaultCategory": "Widgets",
            "DefaultThumbnailPath": "/media/thumbnails/default.png"
        }
    }
}
```

These defaults are applied to content types that do not have an explicit category or thumbnail configured.

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/NDUjn5_KdEM" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/bayT58i7DVY" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>
