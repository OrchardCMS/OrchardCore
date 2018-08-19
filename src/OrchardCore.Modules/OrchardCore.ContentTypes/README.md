# Content Types (OrchardCore.ContentTypes)

## View Components

### SelectContentTypes

Renders an editor to select a list of content types. 
It can optionally filter content types of a specific stereotype.
The editor returns the selection as a `string[]` on the model.

#### Parameters

| Parameter | Type | Description |
| --------- | ---- | ----------- |
| `selectedContentTypes` | `string[]` | The list of content types that should be marked as selected when rendering the editor. |
| `htmlName` | `string` | The name of the model property to bind the result to.
| `stereotype` (optional) | `string` | A stereotype name to filter the list of content types to be able to select. |

#### Sample

```csharp
@await Component.InvokeAsync("SelectContentTypes", new { selectedContentTypes = Model.ContainedContentTypes, htmlName = Html.NameFor(m => m.ContainedContentTypes) })

```

## Migrations

Migration classes can be used to alter the content type definitions, like by adding new __types__, or configuring their __parts__ and __fields__.

### IContentDefinitionManager

This service provides a way to modify the content type definitions. From a migrations class, we can inject an instance of this interface.

```csharp
public class Migrations : DataMigration
{
    IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public int Create()
    {
        // This code will be run when the feature is enabled

        return 1;
    }
}
```

### Creating a new Content Type

The following example creates a new Content Type named `Product`.

```csharp
_contentDefinitionManager.AlterTypeDefinition("Product");
```

### Changing the metadata of a Content Type

To change specific properties of the content type, an argument can be used to configure it:

```csharp
_contentDefinitionManager.AlterTypeDefinition("Product", type => type
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
_contentDefinitionManager.AlterTypeDefinition("Product", type => type
    .WithPart("TitlePart")
);
```

Each part can also be configured in the context of a type. For instance the `AutoroutePart` requires a __Liquid__ template as its pattern to generate custom routes. It's defined in a custom setting for this part.


```csharp
_contentDefinitionManager.AlterTypeDefinition("Product", type => type
    .WithPart("Product", part => part
        // sets the position among other parts
        .WithPosition("2")
        // sets all the settings on the AutoroutePart
        .WithSettings(new AutoroutePartSettings { Pattern = "{{ ContentItem | display_text | slugify }}" })
    )
);
```

For a list of all the settings each type can use, please refer to their respective documentation pages.

### Adding Content Fields to a part

Fields can not be attached directly to a Content Type. To add fields to a content type, create a part using with the same name as the type and add fields to this part. 

```csharp
 _contentDefinitionManager.AlterTypeDefinition("Product", type => type
    .WithPart("Product", part => part
        .WithField("Image", field => field
            .OfType("MediaField")
            .WithDisplayName("Main image"))
        .WithField("Price", field => field
            .OfType("NumericField")
            .WithDisplayName("Price"))
    )
);
```

When added to a part, fields can also have custom settings which for instance will define how the editor will behave, or validation rules. Also refer to their respective documentation pages for a list of possible settings.
