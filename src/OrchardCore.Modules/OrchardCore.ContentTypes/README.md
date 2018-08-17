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

### ContentDefinitionManager - Migrations
#### Example - Using Migrations to create ContentType
```
            _contentDefinitionManager.AlterTypeDefinition("Item", menu => menu
                .Draftable()
                .Versionable()
                .Creatable()
                .Securable()
                .WithPart("TitlePart", part => part.WithPosition("1"))
                .WithPart("AutoRoutePart", part => part.WithPosition("2").WithSettings(new AliasPartSettings { Pattern = "{{ ContentItem | display_text | slugify }}" }))
                .WithPart("AliasPart", part => part.WithPosition("3").WithSettings(new AliasPartSettings { Pattern = "{{ ContentItem | display_text | slugify }}" }))
                .WithPart("HtmlBodyPart", part => part.WithPosition("3").WithSettings(new HtmlBodyPartSettings { Editor = "Wysiwig" }))
                .WithPart("Item", part => part.WithPosition("5"))
            );
```
#### Example - Using Migrations to create Part
```
 _contentDefinitionManager.AlterPartDefinition("Item", part => part
                .WithField("ItemImage", cfg => cfg.OfType("MediaField").WithDisplayName("Main image"))
                .WithField("Price", cfg => cfg.OfType("NumericField").WithDisplayName("Price"))
                .WithField("FromDate", cfg => cfg.OfType("DateField").WithDisplayName("From"))
                .WithField("ToDate", cfg => cfg.OfType("DateField").WithDisplayName("Till"))
                .WithField("Special", cfg => cfg.OfType("BooleanField").WithDisplayName("Special")));
```
#### Adding Fields to a ContentType
Fields can not be attached directly to a ContentType. To add fields to a contenttype, create a part using AlterPartDefinition with the same name as the ContentType, add fields to this Part and then Attach this part to the ContentType. In the example above, ItemImage , Price , FromDate, ToDate, Special fields will be added to "Item" ContentType.   
