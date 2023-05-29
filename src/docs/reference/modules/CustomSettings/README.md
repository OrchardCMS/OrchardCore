# Custom Settings (`OrchardCore.CustomSettings`)

Custom Settings allow a site administrator to create a customized set of properties that are global to the web sites.  
These settings are edited in the standard Settings section and can be protected with specific permissions.

## Creating Custom Settings

Custom Settings are organized in sections. Each section is represented by a Content Type with the `CustomSettings` stereotype.  
When creating such a section, remember to disable `Creatable`, `Listable`, `Draftable` and `Securable` metadata as they don't apply.

!!! warning
    Don't mark any existing Content Type with this `CustomSettings` stereotype, as this will break existing content items of this type.

Custom Settings are then comprised of parts and fields like any other content type.  
Once created, open the Setting menu item and each of these sections should appear alongside the module-provided ones.

### Permissions

Each Custom Settings section gets a dedicated permission to allow specific users to edit it.

To edit this permission open the Roles editor and go to the `OrchardCore.CustomSettings` Feature group.

## Usage

### Liquid

The Custom Settings (like other settings) are available in the `{{ Site.Properties }}` object.  
Each section is made available using its name.

For instance the `HtmlBodyPart` of a custom settings section named `BlogSettings` would be accessible using `{{ Site.Properties.BlogSettings.HtmlBodyPart }}`.

### Code

Custom Settings are a ContentItem, and by accessing it as a `ContentItem` you can access its parts and metadata.

!!! note
    You will need to register your `ContentPart` with Dependency Injection as demonstrated in the [ContentTypes documentation](../ContentTypes/README.md).

Here is an example of getting the `HtmlBodyPart` of a custom settings section named `BlogSettings`:

!!! warning
    These types may be modified in the CMS. It's important to make sure these types will not be modified outside of the development cycle when consuming them in code.

```csharp
public class MyController : Controller
{
    private readonly ISiteService _siteService;
    public MyController(ISiteService siteService)
    {
        _siteService = siteService;
    }
    public async Task<IActionResult> Index()
    {
        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var blogSettings = siteSettings.As<ContentItem>("BlogSettings");
        var blogHtml = blogSettings.As<HtmlBodyPart>();

        return View();
    }
}
```

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/RuDsBx4wdT0" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
