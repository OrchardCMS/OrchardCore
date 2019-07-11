# Custom Settings (OrchardCore.CustomSettings)

## Usage

Custom Settings allows a site administrator to create a customized set of properties that are global to the web sites. These settings are
edited in the standard Settings section and can be protected with specific permissions.

### Creating Custom Settings

Custom Settings are organized in sections. Each section is represented by a Content Type with the `CustomSettings` stereotype.
When creating such a section, remember to disable `Creatable`, `Listable`, `Draftable` and `Securable` metadata as they don't apply.

!!! warning
    Don't mark any existing Content Type with this `CustomSettings` stereotype, as this will break existing content items of this type.

Custom Settings are then comprised of parts and fields like any other content type.
Once created, open the Setting menu item and each of these sections should appear alongside the module-provided ones.

### Permissions

Each Custom Settings sections gets a dedicated permission to allow specific users to edit them.

To edit this permission open the Roles editor and go to the `OrchardCore.CustomSettings` Feature group.

### Templates

The Custom Settings (like other settings) are available in the `{{ Site.Properties }}` object.
Each section is made available using its name.

For instance the `HtmlBodyPart` of a custom settings section named `BlogSettings` would be accessible using `{{ Site.Properties.BlogSettings.HtmlBodyPart }}`.

### Consuming Custom Settings from C#

Required references:

```
OrchardCore.CustomSettings.Abstractions
```

Custom Settings are provided via the `ISiteService` by `OrchardCore.CustomSettings`. The following is an example of its usage in a Controller in MVC.

First, define a ContentPart matching your settings:

```c#
namespace MyNamespace
{
    public class MyCustomSettings : ContentPart
    {
        public TextField MySetting { get; set; }
    }
}
```

Then, register your ContentPart with Dependency Injection:

```c#
public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ContentPart, MyCustomSettings>();
    }
}
```

Finally, grab the `ISiteService` from dependency injection and get the custom settings.

```c#
using OrchardCore.CustomSettings;

namespace MyNamespace {
    public class MyController : Controller
    {
        private readonly ISiteService _siteService;

        public MyController(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task<IActionResult> Index()
        {
            var customSettings3 = await _siteService.GetCustomSettingsAsync<MyCustomSettings>();

            return View();
        }
    }
}
```