# Content Types (`OrchardCore.ContentTypes`)

## View Components

### `SelectContentTypes`

Renders an editor to select a list of content types.  
It can optionally filter content types of a specific stereotype.  
The editor returns the selection as a `string[]` on the model.

#### Parameters

| Parameter | Type | Description |
| --------- | ---- | ----------- |
| `selectedContentTypes` | `string[]` | The list of content types that should be marked as selected when rendering the editor. |
| `htmlName` | `string` | The name of the model property to bind the result to.
| `stereotype` (optional) | `string` | A stereotype name to filter the list of content types available to select. |

#### Sample

```csharp
@await Component.InvokeAsync("SelectContentTypes", new { selectedContentTypes = Model.ContainedContentTypes, htmlName = Html.NameFor(m => m.ContainedContentTypes) })
```

## Migrations

Migration classes can be used to alter the content type definitions, like by adding new __types__, or configuring their __parts__ and __fields__.

### `IContentDefinitionManager`

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
_contentDefinitionManager.AlterTypeDefinition("Product", type => type
    .WithPart("Product")
);

_contentDefinitionManager.AlterPartDefinition("Product", part => part
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

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPart<Product>();
    }
}
```

Finally, here is an example of consuming your Content Item as your Content Part in a Controller.

```csharp
public class ProductController : Controller
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

        var productPart = product.As<Product>();

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

        var productPart = product.As<Product>();
        productPart.Price.Value = price;
        
        product.Apply(productPart) //apply modified part to a content item
        
        await _contentManager.UpdateAsync(product); //update will fire handlers which could alter the content item.

        //validation will cancel changes if product is not valid. It's fired after update since handlers could change the object.
        return await _contentManager.ValidateAsync(product);
    }
}
```
